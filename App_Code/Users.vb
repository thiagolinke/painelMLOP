Imports System.Data.Odbc
Imports Util
Imports System.Collections.Generic
Imports System.Net.Mail
Imports System.DirectoryServices

''' <summary>
''' This class is used to Manage the NetChart users
''' related js file: TABS_INTERFACE/SpecialFunctions/ManageUsers.js
''' related menu entry: Support > Manage USers
''' </summary>
''' <remarks>Created by André Teixeira</remarks>
Public Class Users

	Enum AllowedCustomActions
		Created
		Modified
		Deleted
		Moved
	End Enum

#Region "Users CRUD functions"

	Private Const m_exceptionList As String = "allowed_functions,password_expiration_date,last_login_ip,last_login_time,login_timeout,order_treeview,use_detailed_titles,receive_emails,chart_height,chart_width,workspace,failed_attempts,chart_grid_size,show_chart_legend,store_slot_element,use_excel_output,counters_view"
	Public Shared usersTable As String = "users"

	Dim m_connection As ConnectionManager

	Private Structure m_col_information
		Dim name, default_value, data_type, max_length As String

		Public Sub New(ByRef p_column_name As String, _
					ByRef p_column_default As String, _
					ByRef p_data_type As String, _
					ByRef p_max_length As String)
			name = p_column_name
			default_value = p_column_default
			data_type = p_data_type
			max_length = p_max_length
		End Sub

	End Structure

	''' <summary>
	''' An already opened command
	''' </summary>
	''' <param name="p_command"></param>
	''' <remarks></remarks>
	Public Sub New(ByVal p_connection As ConnectionManager)
		m_connection = p_connection

	End Sub

	''' <summary>
	''' Used to make sure we won't change two usernames at the same time and end up messing the tables
	''' </summary>
	Private Shared userUpdateLock As New Object()

	''' <summary>
	''' Fetchs all users that are not from the remopt group
	''' </summary>
	''' <returns>An hashtable with 2 atributes, users and header.
	''' user is a NxM matrix with all users and it's atributes
	''' header is a Nx4 matrix with the names, datatype, max_length and default value of each column</returns>
	Public Function fetchUsers() As Hashtable
		Dim v_command As New OdbcCommand()
		Dim v_dataReader As OdbcDataReader
		Dim v_userRole As String = HttpContext.Current.Application("userRole")
		Dim v_return As New Hashtable()

		'the v_restrictions string will have the restrictions of each column
		Dim v_restrictions As String = ""

		Dim v_header As New List(Of String())
		Dim v_users As New List(Of String())

		v_return.Add("users", v_users)
		v_return.Add("header", v_header)

		Dim v_columns As List(Of m_col_information) = get_columns_data(v_command)

		Dim v_selectFields As String = ""

		Dim t_innactiveDays As Integer = 360
		Dim t_maxLoginTries As Integer = 4

		If HttpContext.Current.Application("useLDAPLogin") Then
			'We'll display status field at the first column
			v_header.Add(New String() {"status", _
					"varchar", _
					"7", _
					"ACTIVE"})
			v_selectFields &= "IF(user_expiration_date <= Now() Or failed_attempts > " & t_maxLoginTries & ", 'SUSPENDED', IF(DATEDIFF(NOW(), last_login_time) > " & t_innactiveDays & ",'INATIVE','ACTIVE')) as STATUS,"
		End If

		For i As Integer = 0 To v_columns.Count - 1
			v_header.Add(New String() {v_columns(i).name, _
									v_columns(i).data_type, _
									v_columns(i).max_length, _
									v_columns(i).default_value})

			If v_columns(i).name = "password" Then
				v_selectFields &= "'' as password,"
			Else
				v_selectFields &= v_columns(i).name & ","
			End If
		Next

		RemoveLastChar(v_selectFields)

		Dim v_usersList As String = usersList(v_command)

		v_command.CommandText = _
			"SELECT " & v_selectFields & " FROM " & HttpContext.Current.Application("userDB") & ".users WHERE username IN ('" & v_usersList.Replace(",", "','") & "')" & _
			" ORDER BY fullName"
		v_dataReader = m_connection.executeReader(v_command)

		While v_dataReader.Read
			Dim t_currentLine(v_dataReader.VisibleFieldCount - 1) As String

			For i As Integer = 0 To v_dataReader.VisibleFieldCount - 1
				'to avoid errors with date  and null values we'll set the null value as empty and continue the loop
				If v_dataReader.IsDBNull(i) Then
					t_currentLine(i) = v_dataReader(i).ToString
					Continue For
				End If

				Select Case v_dataReader.GetDataTypeName(i)
					Case "datetime"
						t_currentLine(i) = v_dataReader.GetDate(i).ToString("yyyy-MM-dd HH:mm:ss")
					Case "date"
						t_currentLine(i) = v_dataReader.GetDate(i).ToString("yyyy-MM-dd")
					Case Else
						t_currentLine(i) = v_dataReader(i).ToString
				End Select
			Next

			v_users.Add(t_currentLine)
		End While
		v_dataReader.Close()

		Return v_return
	End Function

	''' <summary>
	''' Update the information of the given users
	''' </summary>
	''' <param name="p_users">An array with the new informations of each user, each string of the array must start with the user name and follows with the new values of each column returned by fetchUsers</param>
	''' <returns>Information about the success or failure of the function</returns>
	Public Function updateUsers(ByVal p_users() As String) As String
		Dim v_users() As String = p_users
		Dim v_fields() As String
		Dim v_whereClause As String = ""

		Dim v_command As New OdbcCommand()
		Dim v_dataReader As OdbcDataReader
		Dim v_userRole As String = HttpContext.Current.Application("userRole")
		Dim v_setClause As String
		Dim v_newUserName As String = ""
		Dim v_oldUserName As String
		Dim v_fullName As String = ""
		Dim v_errorMsg As String = ""	'normally the error string will be changed
		Dim v_sendEmail As Boolean
		Dim v_newPassword As String
		'control if an e-mail will be sent to the user
		'informing the changes, used only when username,
		'password or e-mail are changed

		Dim v_userExist As New List(Of String)
		Dim v_passLength As New List(Of String)
		Dim v_userNotFound As New List(Of String)
		Dim v_usersInvalid As New List(Of String)

		Dim allowedToEdit As String = usersList(v_command)

		Dim v_columns As List(Of m_col_information) = get_columns_data(v_command)

		If HttpContext.Current.Application("useLDAPLogin") Then
			'At TIM Italy we have an additional column status, that can be ignored
			v_columns.Insert(0, New m_col_information("status", "", "", ""))
		End If

		Dim v_old_email As String

		For Each t_user As String In v_users
			v_errorMsg = ""
			v_newPassword = ""
			v_fields = t_user.Split("|")
			v_sendEmail = False
			v_oldUserName = v_fields(0)
			v_old_email = get_user_mail(v_command, v_oldUserName)

			If (v_oldUserName.StartsWith("new_user")) Then
				Dim t_result As String = create_user(v_command, v_fields)
				If (t_result <> "") Then
					If t_result.StartsWith("not found:") Then
						v_userNotFound.Add(t_result.Split(":")(1))
					ElseIf t_result.StartsWith("invalid") Then
						v_usersInvalid.Add(t_result.Split(":")(1))
					Else
						v_userExist.Add(t_result)
					End If
				End If
				Continue For
			ElseIf (Not allowedToEdit.Contains(v_oldUserName)) Then
				Return "You can not edit the user:" & v_oldUserName
			End If

			v_whereClause = "username = '" & v_oldUserName & "' "
			v_setClause = "SET "
			'here we remove the first field of the array that was the old username
			v_fields = t_user.Substring(t_user.IndexOf("|") + 1).Split("|")

			'now we check if the number of fields given are the same of the number of fields
			'of the table (the first string is the old username)
			If (v_fields.Length < (v_columns.Count)) Then
				Return "Invalid number of columns!"
			End If

			For i As Integer = 0 To v_columns.Count - 1
				If v_columns(i).name = "status" Then
					Continue For
				End If

				If (v_columns(i).name = "username") Then
					v_newUserName = v_fields(i)

					If (v_newUserName <> v_oldUserName) Then
						'if the username will be changed we make some tests and send an email
						'to the user.
						v_sendEmail = True
						'here we check if the username beeing set already exists.
						v_command.CommandText = "SELECT username FROM " & HttpContext.Current.Application("userDB") & ".users WHERE username='" & v_newUserName & "'"
						v_dataReader = m_connection.executeReader(v_command)
						If (v_dataReader.HasRows) Then
							v_userExist.Add(v_newUserName)
							v_errorMsg = "Username " & v_newUserName & " already exists."
							v_dataReader.Close()
							Exit For
						End If

						v_dataReader.Close()

						SyncLock userUpdateLock
							'updateUserNameOnNetchartTables(v_oldUserName, v_newUserName)
						End SyncLock

					End If
				End If

				If (v_columns(i).name = "fullname") Then
					v_fullName = v_fields(i)
				End If

				'to the password field we need an especial case
				If (v_columns(i).name = "password") Then
					'if the password will be changed we need to make some tests and send an
					'email to the user
					If (v_fields(i).Length > 0) Then
						v_sendEmail = True
						v_newPassword = DecriptString("new_password", v_fields(i))
						v_setClause &= v_columns(i).name & "=sha('" & v_newPassword & "'),"
					End If

					If (v_newPassword.Length > 25) Then
						v_passLength.Add(v_newUserName)
						v_errorMsg = "The maximum password lenght is 25 caracters."
						Exit For
					End If

					Continue For
				End If

				' Change to comma if the user inserted semicolon
				If (v_columns(i).name = "allowed_techs") Then
					v_fields(i) = v_fields(i).Replace(";", ",")
				End If

				If (v_fields(i) = "") Then
					If (v_columns(i).default_value = "NULL") Then
						v_setClause &= v_columns(i).name & "=NULL,"
					Else
						v_setClause &= v_columns(i).name & "='" & v_columns(i).default_value & "',"
					End If
				Else
					v_setClause &= v_columns(i).name & "='" & v_fields(i).Replace("'", "\'") & "',"
				End If

			Next

			If (v_errorMsg <> "") Then
				Continue For
			End If

			RemoveLastChar(v_setClause)

			If HttpContext.Current.Application("useLDAPLogin") AndAlso _
					v_setClause.Contains("rights_manager='0'") AndAlso _
					v_setClause.Contains("rights_create_region='0'") AndAlso _
					v_setClause.Contains("rights_manage_users='1'") AndAlso _
					v_setClause.Contains("rights_crontab='0'") Then
				v_setClause &= ",allowed_functions='MANAGE_USERS,TROUBLETICKETS,ACCOUNT_PREFS'"
			End If

			v_command.CommandText = "UPDATE " & HttpContext.Current.Application("userDB") & ".users " & v_setClause & " WHERE " & v_whereClause
			m_connection.executeNonQuery(v_command)

			'addToHistory(v_command, AllowedCustomActions.Modified, v_oldUserName, v_command.CommandText)

			Dim v_new_email As String = get_user_mail(v_command, v_newUserName)

			If (v_old_email <> v_new_email) Then
				v_sendEmail = True
			End If

			If (v_sendEmail) Then
				Dim t_emailChanged As Boolean = False
				Dim t_userName As String = ""
				If v_old_email <> v_new_email Then
					t_emailChanged = True
				End If
				If v_oldUserName <> v_newUserName Then
					t_userName = v_newUserName
				End If

				'SendUserAlterationEmail(AllowedCustomActions.Modified, v_new_email, t_userName, v_newPassword, v_fullName, t_emailChanged)
			End If
		Next

		Dim v_responseScript As String

		If (v_userExist.Count = 0 And v_passLength.Count = 0 And v_userNotFound.Count = 0 And v_usersInvalid.Count = 0) Then
			Return "Changes made with sucess!"
		Else
			Dim t_errorMessage As String = ""
			If (v_userExist.Count > 0) Then
				t_errorMessage &= "User(s) " & Join(v_userExist.ToArray, ",") & " already exist. "
			End If

			If (v_userNotFound.Count > 0) Then
				t_errorMessage &= "User(s) " & Join(v_userNotFound.ToArray, ",") & " not found at the LDAP database. "
			End If

			If (v_usersInvalid.Count > 0) Then
				t_errorMessage &= "User(s) " & Join(v_usersInvalid.ToArray, ",") & " have an invalid name. "
			End If

			If (v_passLength.Count > 0) Then
				t_errorMessage &= "Password length of the users " & Join(v_passLength.ToArray, ",") & " too long (max 25 characters). "
			End If

			t_errorMessage &= "Other users changed with sucess."

			Return t_errorMessage
		End If

	End Function

	''' <summary>
	''' Delete entries from the users table
	''' </summary>
	''' <returns></returns>
	''' <remarks></remarks>
	Public Function deleteUsers(ByVal p_users() As String) As String
		Dim v_connection As New OdbcConnection(HttpContext.Current.Application("connectionString_user"))
		Dim v_command As New OdbcCommand()
		Dim v_user_mail As String
		v_command.Connection = v_connection
		v_connection.Open()

		Dim v_user_names As String = ""
		Dim v_allowedToEdit As String = usersList(v_command)

		Dim v_usersFailed As New List(Of String)

		For Each t_user As String In p_users
			v_user_names &= "'" & t_user & "',"

			If (Not v_allowedToEdit.Contains(t_user)) Then
				v_usersFailed.Add(t_user)
			End If

			v_user_mail = get_user_mail(v_command, t_user)
			If (v_user_mail <> "") Then
				'SendUserAlterationEmail(AllowedCustomActions.Deleted, v_user_mail, t_user, "")
			End If
		Next

		RemoveLastChar(v_user_names)

		v_command.CommandText = "DELETE FROM " & HttpContext.Current.Application("userDB") & ".users WHERE username IN (" & v_user_names & ")"
		v_command.ExecuteNonQuery()

		'addToHistory(v_command, AllowedCustomActions.Deleted, v_user_names.Replace("'", ""), v_command.CommandText)

		v_connection.Dispose()

		If v_usersFailed.Count > 0 Then
			deleteUsers = "You can not removed the user(s): " & Join(v_usersFailed.ToArray, "," & " the other users where removed with success.")
		Else
			deleteUsers = "Users removed with success."
		End If

	End Function

	Private Function create_user(ByRef p_command As OdbcCommand, ByVal p_values() As String) As String
		Dim v_column_names() As String
		Dim v_values() As String
		Dim v_user_pass As String = ""
		Dim v_user_mail As String = ""
		Dim v_user_name As String = ""
		Dim v_dataReader As OdbcDataReader
		Dim v_userFullName As String = ""

		'removing any unecessary spaces from the values
		For i As Integer = 0 To p_values.Length - 1
			p_values(i) = p_values(i).Trim()
		Next

		Dim v_col_info As List(Of m_col_information) = get_columns_data(p_command)

		ReDim v_values(v_col_info.Count - 1)
		ReDim v_column_names(v_col_info.Count - 1)

		'This offset is used to ignore some columns(old_username and status on LDAP case)
		Dim v_offset As Integer
		If HttpContext.Current.Application("useLDAPLogin") Then
			'We'll display status field at the first column
			v_offset = 2
		Else
			v_offset = 1
		End If

		For i As Integer = 0 To v_col_info.Count - 1
			'escaping possible quotes in the values
			p_values(i + 1) = p_values(i + v_offset).Trim().Replace("'", "\'")

			v_column_names(i) = v_col_info(i).name

			If (v_col_info(i).name = "email") Then
				v_user_mail = p_values(i + v_offset).Trim()
			End If
			If (v_col_info(i).name = "username") Then
				'usernames hould always have lowercase caracters only
				p_values(i + 1) = p_values(i + 1).ToLower
				v_user_name = p_values(i + v_offset)
			End If

			If (v_col_info(i).name = "fullname") Then
				v_userFullName = p_values(i + v_offset)
			End If

			If (v_col_info(i).name = "password") Then
				v_user_pass = DecriptString("new_password", p_values(i + 1))
				v_values(i) = "sha('" & v_user_pass & "')"
			Else
				v_values(i) = "'" & p_values(i + v_offset) & "'"
			End If
		Next

		If v_user_name.StartsWith("new_user") Then
			Return "invalid:" & v_user_name
		End If

		If HttpContext.Current.Application("useLDAPLogin") Then
			Dim d As New DirectoryEntry(HttpContext.Current.Application("ldapAddress"), v_user_name, "", AuthenticationTypes.None)
			v_user_pass = "*Same of your LDAP account*"

			Dim s As New DirectorySearcher(d)
			s.Filter = "(uid=" & v_user_name & ")"
			s.PropertiesToLoad.Add("cn")
			s.PropertiesToLoad.Add("department")
			s.PropertiesToLoad.Add("company")
			s.PropertiesToLoad.Add("mail")
			s.PropertiesToLoad.Add("mobile")
			s.PropertiesToLoad.Add("tigpwdexpirationdate")
			s.PropertiesToLoad.Add("accountExpires")
			s.PropertiesToLoad.Add("manager")

			Dim r As SearchResult
			Try
				r = s.FindOne
			Catch ex As Exception
				Throw New Exception("Could not connect to LDAP server.")
			End Try

			If r Is Nothing Then
				Return "not found:" & v_user_name
			End If

			Dim v_isGguUser As Boolean = True

			For i As Integer = 0 To v_column_names.Length - 1
				Dim t_property As String

				Select Case v_column_names(i)
					Case "fullname"
						t_property = "cn"
						v_userFullName = r.Properties(t_property)(0).ToString.Replace("'", "\'")
					Case "email"
						t_property = "mail"
						v_user_mail = r.Properties(t_property)(0).ToString.Replace("'", "\'")
					Case "phone"
						t_property = "mobile"
					Case "company"
						t_property = "company"
					Case "manager"
						Dim t_managerId As String = r.Properties("manager")(0).Substring(4).Split(",")(0)
						s.Filter = "(uid=" & t_managerId & ")"
						s.PropertiesToLoad.Add("cn")
						Dim v_joinManager As SearchResult = s.FindOne
						Dim t_managerName As String = v_joinManager.Properties("cn")(0).replace("'", "\'")
						v_values(i) = "'" & t_managerName & "'"
						t_property = ""
					Case "user_group"
						t_property = "department"
					Case "user_expiration_date"
						Dim expireDate As DateTime = DateTime.Now().AddYears(1)
						v_values(i) = "'" & expireDate.ToString("yyyy-MM-dd") & "'"
						t_property = ""
					Case "rights_manager"
						If v_values(i) <> "'0'" Then v_isGguUser = False
					Case "rights_create_region"
						If v_values(i) <> "'0'" Then v_isGguUser = False
					Case "rights_manage_users"
						If v_values(i) <> "'1'" Then v_isGguUser = False
					Case "rights_crontab"
						If v_values(i) <> "'0'" Then v_isGguUser = False
					Case Else
						t_property = ""
				End Select

				If (t_property <> "" AndAlso r.Properties(t_property).Count > 0) Then
					v_values(i) = "'" & r.Properties(t_property)(0).ToString.Replace("'", "\'") & "'"
				End If
			Next

			If v_isGguUser Then
				Dim v_newLength As Integer = v_column_names.Length
				ReDim Preserve v_values(v_newLength)
				ReDim Preserve v_column_names(v_newLength)

				v_column_names(v_newLength) = "allowed_functions"
				v_values(v_newLength) = "'MANAGE_USERS,TROUBLETICKETS,ACCOUNT_PREFS'"
			End If
		End If

		p_command.CommandText = "SELECT username FROM " & HttpContext.Current.Application("userDB") & ".users WHERE username='" & v_user_name & "'"
		v_dataReader = m_connection.executeReader(p_command)
		If (v_dataReader.HasRows) Then
			v_dataReader.Close()
			Return v_user_name
		End If
		v_dataReader.Close()

		p_command.CommandText = "INSERT INTO " & HttpContext.Current.Application("userDB") & ".users (" & Join(v_column_names, ",") & ") VALUES (" & Join(v_values, ",") & ")"
		m_connection.executeNonQuery(p_command)
		'SendUserAlterationEmail(AllowedCustomActions.Created, v_user_mail, v_user_name, v_user_pass, v_userFullName)

		'addToHistory(p_command, AllowedCustomActions.Created, v_user_name, p_command.CommandText)

		Return ""
	End Function

	Private Function usersList(ByRef p_command As OdbcCommand) As String
		Dim v_dataReader As OdbcDataReader
		usersList = ""

		p_command.CommandText = _
				"SELECT username FROM " & HttpContext.Current.Application("userDB") & _
				".users WHERE (user_group <> 'RemOpt' OR user_group IS NULL) "

		v_dataReader = m_connection.executeReader(p_command)

		If v_dataReader.HasRows() Then
			While (v_dataReader.Read())
				usersList &= v_dataReader.Item("username").ToString & ","
			End While
		End If

		v_dataReader.Close()
		RemoveLastChar(usersList)

	End Function

	Private Function get_user_mail(ByRef p_command As OdbcCommand, ByVal p_username As String) As String
		Dim v_dataReader As OdbcDataReader
		get_user_mail = ""

		p_command.CommandText = "SELECT email FROM " & HttpContext.Current.Application("userDB") & ".users WHERE username='" & p_username & "'"
		v_dataReader = m_connection.executeReader(p_command)

		If (v_dataReader.HasRows) Then
			get_user_mail = v_dataReader(0).ToString
		End If
		v_dataReader.Close()

	End Function

	Private Function get_columns_data(ByRef p_command As OdbcCommand) As List(Of m_col_information)
		Dim m_columns As List(Of m_col_information) = New List(Of m_col_information)
		Dim v_dataReader As OdbcDataReader

		p_command.CommandText = "SELECT COLUMN_NAME, " & _
				"COLUMN_DEFAULT, " & _
				"DATA_TYPE, " & _
				"CHARACTER_MAXIMUM_LENGTH " & _
				"FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = '" & HttpContext.Current.Application("userDB") & "' AND table_name = 'users'"
		v_dataReader = m_connection.executeReader(p_command)

		Dim v_column_default As String
		Dim v_max_length As String

		While (v_dataReader.Read())
			If (isOnExceptionList(v_dataReader.GetValue(0))) Then
				Continue While
			End If

			If (v_dataReader.IsDBNull(1)) Then
				v_column_default = "NULL"
			Else
				v_column_default = v_dataReader.GetValue(1).ToString
			End If

			'if the DATA_TYPE isnt an string type it doesnt have
			'max length, so we estimate it 
			v_max_length = v_dataReader.GetValue(3).ToString
			If (v_max_length = "") Then
				Select Case v_dataReader.GetValue(2).ToString
					Case "smallint"
						v_max_length = "5"
					Case "int"
						v_max_length = "9"
					Case "bigint"
						v_max_length = "18"
					Case "tinyint"
						v_max_length = "2"
					Case Else
						v_max_length = ""
				End Select
			Else
			End If

			m_columns.Add(New m_col_information( _
																			v_dataReader.GetValue(0).ToString, _
																			v_column_default, _
																			v_dataReader.GetValue(2).ToString, _
																			v_max_length))

		End While

		v_dataReader.Close()

		Return m_columns
	End Function

	Private Function isOnExceptionList(ByVal p_column As String) As Boolean
		' Assume we will find the string.
		isOnExceptionList = True
		Dim v_exceptions As New List(Of String)
		v_exceptions.AddRange(m_exceptionList.Split(","))

		If HttpContext.Current.Application("useLDAPLogin") Then
			v_exceptions.Add("password")
			v_exceptions.Add("password_days_to_expire")
			v_exceptions.Remove("last_login_time")
		End If

		If HttpContext.Current.Application("tool_used") = "comcel" Then
			v_exceptions.Remove("allowed_functions")
		End If

		For Each t_string As String In v_exceptions
			If t_string = p_column Then Exit Function
		Next
		' We didn't find the string.
		isOnExceptionList = False
	End Function

	''' <summary>
	''' Determines if the current logged in user has rights to edit the users tables
	''' </summary>
	Public Function currentUserHasRights() As Boolean
		Dim v_currentUserName As String = validateuser()
		Dim v_dataReader As OdbcDataReader
		Dim v_command As New OdbcCommand
		Try
			v_command.CommandText = "SELECT username FROM " & HttpContext.Current.Application("userDB") & ".users WHERE username='" & v_currentUserName & "' AND rights_manage_users = '1'"
			v_dataReader = m_connection.executeReader(v_command)
			currentUserHasRights = v_dataReader.HasRows
			v_dataReader.Close()
		Catch ex As Exception
			currentUserHasRights = False
		End Try
	End Function

	''' <summary>
	''' </summary>
	''' <param name="p_action">The action can be modified, deleted or created</param>
	''' <remarks>
	'''</remarks>
	Private Sub SendUserAlterationEmail(ByVal p_action As AllowedCustomActions, _
																																					ByVal p_modifiedUserEmail As String, _
																																					ByVal p_username As String, ByVal p_password As String, _
																																					Optional ByVal p_completeUserName As String = "", Optional ByVal p_emailChanged As Boolean = False)

		'Gets user email address
		Dim v_userEmail As String = HttpContext.Current.Request.Cookies("USEREMAIL").Value
		'Creates the email body
		Dim v_mail As New MailMessage()
		v_mail.From = New MailAddress(HttpContext.Current.Application("smtpUser"), "NetChart User Alteration")
		v_mail.Subject = "User Modification"
		v_mail.ReplyTo = New MailAddress("support@remopt.com")

		Dim v_url As String = Util.GetPublicAdress(HttpContext.Current.Application("tool_used"))
		If (v_url <> "") Then
			v_url &= "default.aspx"
		End If

		If (p_action = AllowedCustomActions.Created) Then
			v_mail.Body = _
					"Hello" & IIf(p_completeUserName <> "", " " & p_completeUserName, "") & "," & Chr(10) & Chr(10) & _
					"We would like to welcome you to NetChart Multivendor! " & _
					"Bellow you find information for accessing the tool:" & Chr(10) & Chr(10) & _
					"username : " & p_username & Chr(10) & _
					"password: " & p_password & Chr(10) & _
					IIf(v_url <> "", "address: " & v_url & Chr(10) & Chr(10), "") & _
					IIf(HttpContext.Current.Application("useLDAPLogin"), "You will be asked to change your password on your first login." & Chr(10) & Chr(10), "") & _
					"Please use Chrome or Firefox to have a better experience with NetChart. You can download them at:" & Chr(10) & _
					"Chrome:  https://www.google.com/chrome" & Chr(10) & Chr(10) & _
					"Best Regards," & Chr(10) & Chr(10) & _
					"NetChart Support Team"
		ElseIf (p_action = AllowedCustomActions.Deleted) Then
			v_mail.Body = _
					"Hello" & IIf(p_completeUserName <> "", " " & p_completeUserName, "") & "," & Chr(10) & Chr(10) & _
					"Your user """ & p_username & """ was removed from NetChart Multivendor and you can't access the tool using this login anymore." & Chr(10) & _
					"Any questions or inquiries please reply this e-mail." & Chr(10) & Chr(10) & _
					"Best Regards," & Chr(10) & Chr(10) & _
					"NetChart Support Team"
		Else 'modified
			v_mail.Body = _
					"Hello" & IIf(p_completeUserName <> "", " " & p_completeUserName, "") & "," & Chr(10) & Chr(10) & _
					"Your user on NetChart Multivendor was modified." & Chr(10) & _
					"The new informations are:" & Chr(10) & Chr(10) & _
					IIf(p_username <> "", "username : " & p_username & Chr(10), "") & _
					IIf(p_emailChanged, "email : " & p_modifiedUserEmail & Chr(10), "") & _
					IIf(p_password <> "", "password: " & p_password & Chr(10), "") & _
					"Any questions or inquiries please reply this e-mail." & Chr(10) & Chr(10) & _
					"Best Regards," & Chr(10) & Chr(10) & _
					"NetChart Support Team"
		End If


		v_mail.Body &= Chr(10) & Chr(10) & "Date: " & Date.Now.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture) & Chr(10) & _
		"Project: " & HttpContext.Current.Application("userRole")

		v_mail.To.Add(v_userEmail.Trim())
		v_mail.To.Add(p_modifiedUserEmail.Trim())
		v_mail.From = New MailAddress(HttpContext.Current.Application("smtpUser"))
		v_mail.Priority = MailPriority.High

		Try
			'EmailManager.sendEmail(v_mail)
		Catch ex As Exception
		End Try

	End Sub

	Private Sub addToHistory(ByRef p_command As OdbcCommand, _
														ByVal p_action As AllowedCustomActions, _
														ByVal p_userModified As String, _
														ByVal p_query As String)

		Dim v_query As String = p_query.Replace("'", "")
		Dim v_action As String = ""

		If (p_action = AllowedCustomActions.Created) Then
			v_action = "create"
		ElseIf p_action = AllowedCustomActions.Modified Then
			v_action = "change"
		ElseIf (p_action = AllowedCustomActions.Deleted) Then
			v_action = "delete"
		Else
			v_action = "undef"
		End If

		p_command.CommandText = "INSERT INTO " & HttpContext.Current.Application("userDB") & ".`users_modifications` " & _
		"(manager,username,action,datetime,query) VALUES" & _
		"('" & validateuser() & "','" & p_userModified & "','" & v_action & "','" & Date.Now.ToString("yyyy-MM-dd HH:mm:ss") & "','" & v_query & "')"
		m_connection.executeNonQuery(p_command)

	End Sub

#Region "Functions used to change the username"

	Public Shared Function tablesAndColumns(ByVal p_forGenericDb As Boolean) As String()()
		Dim v_tables As New List(Of String())

		If p_forGenericDb Then
			v_tables.Add(New String() {"function_preferences", "username"})
		Else
			v_tables.Add(New String() {"netchart_user_comments", "username"})
			v_tables.Add(New String() {"netchart_regions", "owner"})
			v_tables.Add(New String() {"netchart_menu_deleted", "owner"})
			v_tables.Add(New String() {"netchart_menu_deleted", "deletedBy"})
			v_tables.Add(New String() {"netchart_menu", "owner"})
			v_tables.Add(New String() {"netchart_chart_series_deleted", "owner"})
			v_tables.Add(New String() {"netchart_chart_series", "owner"})
			v_tables.Add(New String() {"function_preferences", "username"})
			v_tables.Add(New String() {"_kpis_visibility", "username"})
			v_tables.Add(New String() {"_kpis_visibility", "owner"})
			v_tables.Add(New String() {"_kpis_pvt", "owner"})
			v_tables.Add(New String() {"_kpis_history", "owner"})
			v_tables.Add(New String() {"_kpis_history", "username"})
		End If

		Return v_tables.ToArray
	End Function


#End Region

#End Region
	Shared Function getUsersReader(ByVal p_command As OdbcCommand, _
														ByVal p_fieldsToFetch As String(), _
														Optional ByVal p_whereClause As String = "", _
														Optional ByVal p_orderClause As String = "") As OdbcDataReader

		Dim v_command As OdbcCommand = p_command
		Dim v_dataReader As OdbcDataReader

		If p_orderClause <> "" Then
			p_orderClause = " ORDER BY " & p_orderClause
		ElseIf Array.IndexOf(p_fieldsToFetch, "fullname") <> -1 Then
			p_orderClause = " ORDER BY fullname "
		End If

		v_command.CommandText = "SELECT " & String.Join(",", p_fieldsToFetch) & _
														" FROM " & HttpContext.Current.Application("userDB") & ".users" & _
														IIf(p_whereClause <> "", " WHERE " & p_whereClause, "") & _
														p_orderClause
		v_dataReader = v_command.ExecuteReader

		Return v_dataReader
	End Function

	Shared Function getUsers(ByVal p_command As OdbcCommand, _
														ByVal p_fieldsToFetch As String(), _
														Optional ByVal p_whereClause As String = "", _
														Optional ByVal p_orderClause As String = "") As List(Of Dictionary(Of String, String))

		Dim v_dataReader As OdbcDataReader
		Dim v_usersList As New List(Of Dictionary(Of String, String))

		If p_orderClause <> "" Then
			p_orderClause = " ORDER BY " & p_orderClause
		ElseIf Array.IndexOf(p_fieldsToFetch, "fullname") <> -1 Then
			p_orderClause = " ORDER BY fullname "
		End If

		'Fidelis on 2015-11-23: i put this connectionManager here and i created a query here instead of inside of getUsersReader, that will not treat local user database issue
		'I let the old method(getUsersReader) above for compatible old codes issues.
		Using connection As New ConnectionManager()

			p_command.CommandText = "SELECT " & String.Join(",", p_fieldsToFetch) & _
														" FROM " & HttpContext.Current.Application("userDB") & ".users" & _
														IIf(p_whereClause <> "", " WHERE " & p_whereClause, "") & _
														p_orderClause

			v_dataReader = connection.executeReader(p_command)

			Dim t_user As Dictionary(Of String, String)

			While (v_dataReader.Read)
				t_user = New Dictionary(Of String, String)
				For i As Integer = 0 To p_fieldsToFetch.Length - 1
					t_user.Add(p_fieldsToFetch(i), v_dataReader(i).ToString)
				Next
				v_usersList.Add(t_user)
			End While
		End Using

		Return v_usersList
	End Function

	''' <summary>
	''' Function checks if user has manager's rights
	''' </summary>
	''' <param name="p_userName">User name</param>
	''' <returns>True if user has manager's rights</returns>
	''' <remarks></remarks>
	''' Created by Vitalii - 2010-July-09
	''' Modified by Vitalii - 2010-July-09
	Shared Function isManagerUser(ByVal p_userName As String) As Boolean
		Dim v_command As New OdbcCommand()

		Dim v_dataReader As OdbcDataReader
		Dim v_manager As Boolean = False

		'Let's check here if our user is manager or not (kind of super user)
		v_command.CommandText = _
		"SELECT DISTINCT rights_manager " & _
		"FROM " & HttpContext.Current.Application("userDB") & ".users " & _
		"WHERE username='" + p_userName + "'; "

		Using connection As New ConnectionManager()
			v_dataReader = connection.executeReader(v_command)

			Dim t_manager As Integer
			If v_dataReader.HasRows Then

				If Not IsDBNull(v_dataReader(0)) Then
					t_manager = v_dataReader(0)
					If t_manager = 1 Then
						v_manager = True
					End If
				End If

			End If

			v_dataReader.Close()
		End Using

		Return v_manager
	End Function

	Public Shared Function getUsersNames(Optional p_includeRemoptUsers As Boolean = False) As List(Of String)
		Dim v_command As New OdbcCommand()
		Dim v_users As New List(Of String)

		Using connection As New ConnectionManager()
			Dim v_dataReader As OdbcDataReader

			v_command.CommandText = _
				"SELECT username " & _
				"FROM " & HttpContext.Current.Application("userDB") & ".users "

			v_dataReader = connection.executeReader(v_command)

			While (v_dataReader.Read)
				If (Not v_users.Contains(v_dataReader("username"))) Then
					v_users.Add(v_dataReader("username"))
				End If
			End While

			v_dataReader.Close()
		End Using

		Return v_users
	End Function

	Public Shared Function getLoginCookies(ByVal p_userName As String, ByVal p_userTableCommand As OdbcCommand) As Hashtable
		Dim v_cookies As New Hashtable

		Dim v_authCookie As String = "MLOP"
		Dim v_authCookieName As String = "ASPXVAL"
		Dim v_authCookieValue As String = ""

		Dim userEmail As String = ""
		Dim userFullName As String = ""
		Dim v_team As String = ""
		Dim v_dataReader As OdbcDataReader

		p_userTableCommand.CommandText = _
			"SELECT fullname, email, order_treeview, chart_height, chart_width, " & _
			"allowed_functions, allowed_techs, " & _
			"use_detailed_titles, show_chart_legend, chart_grid_size, store_slot_element, use_excel_output, treeview_mode, excelExtension " & _
			"FROM " & HttpContext.Current.Application("userDB") & ".users " & _
			"WHERE username='" & p_userName & "' "

		v_dataReader = p_userTableCommand.ExecuteReader

		If Not v_dataReader.Read Then
			Return v_cookies
		End If

		If Not IsDBNull(v_dataReader("email")) Then
			userEmail = v_dataReader("email")
		End If
		If Not IsDBNull(v_dataReader("fullname")) Then
			userFullName = v_dataReader("fullname")
		End If
		If Not IsDBNull(v_dataReader("team")) Then
			v_team = v_dataReader("team")
		End If

		v_authCookieValue = v_authCookieName & "=" & getcookievalue(p_userName) & "&USERNAME=" & p_userName

		v_cookies.Add(v_authCookie, v_authCookieValue)
		v_cookies.Add("USEREMAIL", "" & userEmail)
		v_cookies.Add("USERNAME", "" & p_userName)
		v_cookies.Add("TIME", "" & v_team)

		Return v_cookies
	End Function

End Class

Public Class UsersManageConfiguration

	Public Function runFromInterface(ByVal p_context As Web.HttpRequest) As Hashtable
		Dim v_model As New Hashtable
		Dim v_executableCode As New ArrayList()
		Dim v_data As New Hashtable

		v_model.Add("data", v_data)
		v_model.Add("executableCodeArray", v_executableCode)

		Dim v_action As String = p_context("action")

		Using connection As New ConnectionManager()
			If v_action = "getUsersKpi" Then
				Dim v_fieldsUsers() As String = {"username", "fullname"}
				v_data.Add("users", Users.getUsers(connection.createCommand, v_fieldsUsers))
				v_executableCode.Add("this.contextObject.populateUsers(data.users);")
			Else
				v_model = Me.runManageUsersFunction(p_context, connection, v_action)
			End If
		End Using

		Return v_model

	End Function

	Private Function runManageUsersFunction(ByVal p_context As Web.HttpRequest, ByVal p_connection As ConnectionManager, ByVal v_action As String) As Hashtable
		Dim v_model As New Hashtable
		Dim v_executableCode As New ArrayList()
		Dim v_data As New Hashtable

		v_model.Add("data", v_data)
		v_model.Add("executableCodeArray", v_executableCode)

		Dim v_usersManager As New Users(p_connection)
		If Not v_usersManager.currentUserHasRights() Then
			v_model("executableCodeArray").add("this.__netchartFunction.printSuccessMessage('You dont have the rights to acess this function.');")
			Return v_model
		End If
		Select Case v_action
			Case "populate_users"
				'populate users always return the javascript code to execute
			Case "save_modifications"
				v_model("data").add("message", v_usersManager.updateUsers(p_context("users").Split("$")))
				v_model("executableCodeArray").add("this.__netchartFunction.printSuccessMessage(data.message)")
			Case "delete_users"
				v_model("data").add("message", v_usersManager.deleteUsers(p_context("users").Split("$")))
				v_model("executableCodeArray").add("this.__netchartFunction.printSuccessMessage(data.message)")
			Case Else
				Throw New ArgumentException("Invalid action (" & v_action & ") for users manager.")
		End Select

		'only on export users case we don't need to repopulate the users
		If v_action <> "export_users" And v_action <> "update_ldap_users" Then
			v_model("data").add("tableData", v_usersManager.fetchUsers())
			'André Martins on 2014-06-13: Added profiles to various projects
			v_model("executableCodeArray").add("this.__netchartFunction.setProfiles('" & HttpContext.Current.Application("tool_used") & "');")
			v_model("executableCodeArray").add("this.__netchartFunction.populateUsersData(data.tableData.header,data.tableData.users);")
		End If

		Return v_model
	End Function

End Class