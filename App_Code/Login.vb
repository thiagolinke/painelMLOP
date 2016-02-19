Imports Microsoft.VisualBasic
Imports System.Security.Cryptography.X509Certificates
Imports System.DirectoryServices
Imports System.Net.Mail
Imports System.Data.Odbc
Imports Util

Public Class Login

	Public Enum LoginResult
		sucess
		invalid_pass
		invalid_user
		new_pass
	End Enum

	Private m_config As LoginConfiguration

	Public Sub New(ByVal p_loginConfiguration As LoginConfiguration)
		Me.m_config = p_loginConfiguration
	End Sub
	''' <summary>
	''' validate username and password fields
	''' </summary>
	''' <returns>String v_message - messagem empty means no error, otherwise will inform what error occurs</returns>
	Private Function validateLoginFields() As String
		Dim v_message As String = ""

		If Me.m_config.m_username = "" Or Me.m_config.m_password = "" Then
			v_message = " Missing fields."
		ElseIf Len(Me.m_config.m_username) > 20 Then

			v_message = " User name too long (max 20 characters)."

		End If
		Return v_message
	End Function

	''' <summary>
	''' check if user account is blocked
	''' </summary>
	''' <returns>String v_message - messagem empty means no error, otherwise will inform what error occurs</returns>
	''' <remarks></remarks>
	Private Function accountIsBlocked() As String
		Dim v_errorMessage As String = ""
		Dim v_dataReader As OdbcDataReader
		Dim v_maxLoginTries As Integer = 4

		m_config.m_command.CommandText = "SELECT failed_attempts " & _
		"FROM " & Me.m_config.m_userDB & ".users " & _
		"WHERE username = '" & Me.m_config.m_username & "' "
		v_dataReader = m_config.m_connectionManager.executeReader(m_config.m_command)

		If (v_dataReader.Read) Then
			If (v_dataReader("failed_attempts").ToString > v_maxLoginTries) Then
				v_errorMessage = "Account blocked for too many invalid login tries. Please click at Forgot your password link to receive a new one."
			End If
		End If
		v_dataReader.Close()
		Return v_errorMessage
	End Function


	''' <summary>
	''' do some necessary updates in user and user_statistic tables post success login attempt
	''' </summary>
	''' <remarks></remarks>
	Private Sub updatePostSuccessLoginAttempt()

		'We need to update users_statistics table and users table         
		updateStatisticsTable(LoginResult.sucess)
		m_config.m_command.CommandText = _
			"UPDATE " & m_config.m_userDB & ".users " & _
			"SET failed_attempts = 0" & _
			" WHERE username='" & m_config.m_username & "';"
		m_config.m_connectionManager.executeNonQuery(m_config.m_command)
	End Sub
	''' <summary>
	''' do some necessary updates in user and user_statistic tables post fail login attempt
	''' </summary>
	''' <remarks></remarks>
	Private Sub updatePostFailLoginAttempt()
		Dim v_dataReader As OdbcDataReader
		m_config.m_command.CommandText = _
				"UPDATE " & m_config.m_userDB & ".users " & _
				"SET failed_attempts = failed_attempts + 1" & _
				" WHERE username='" & m_config.m_username & "';"

		m_config.m_connectionManager.executeNonQuery(m_config.m_command)

		m_config.m_command.CommandText = "SELECT u.email " & _
		"FROM " & m_config.m_userDB & ".users AS u " & _
		"WHERE u.username='" & m_config.m_username & "' "
		v_dataReader = m_config.m_connectionManager.executeReader(m_config.m_command)

		If (v_dataReader.Read) Then
			v_dataReader.Close()
			updateStatisticsTable(LoginResult.invalid_pass)
		Else
			v_dataReader.Close()
			updateStatisticsTable(LoginResult.invalid_user)
		End If
	End Sub

	''' <summary>
	''' Add a row into the users statistics table with some information about a login try
	''' </summary>
	''' <param name="p_type">The result of the login try</param>
	''' <remarks>
	''' Created by André Teixeira on 2011-11-09 (Mantis issue: 01392)
	''' Code Refactoring by Fidelis on 2014-12-05
	''' </remarks>
	Private Sub updateStatisticsTable(ByVal p_type As LoginResult)

		If p_type = LoginResult.sucess Then
			m_config.m_command.CommandText = "UPDATE " & m_config.m_userDB & ".users SET last_login_ip='" & HttpContext.Current.Request.UserHostAddress & "', last_login_time='" & Date.Now.ToString("yyyy-MM-dd HH:mm:ss") & "' WHERE username='" & m_config.m_username & "' LIMIT 1;"
			m_config.m_connectionManager.executeNonQuery(m_config.m_command)
		End If
		m_config.m_command.CommandText = _
				"INSERT IGNORE INTO " & m_config.m_userDB & ".user_statistics(user_name, user_ip, tool_used, start_time, response_type) " & _
				"VALUES('" & m_config.m_username & "'," & _
				"'" & HttpContext.Current.Request.UserHostAddress & "'," & _
				"'" & HttpContext.Current.Application("tool_used").ToString() & "'," & _
				"'" & Date.Now.ToString("yyyy-MM-dd HH:mm:ss") & "'," & _
				"'" & p_type.ToString & "');"
		m_config.m_connectionManager.executeNonQuery(m_config.m_command)
	End Sub

	''' <summary>
	''' handle user login event
	''' </summary>
	''' <returns>Hashtable with data information about login and executable code array in view interface</returns>
	Public Function login() As Hashtable

		Dim v_hashToReturn As Hashtable = New Hashtable
		Dim v_hashData As Hashtable = New Hashtable
		Dim v_executableCodeArray As ArrayList = New ArrayList
		Dim v_cookies As Hashtable
		Dim v_executableCode As String = ""
		Dim v_errorMessage As String
		Dim qrySELECT As String

		'Fidelis when refactoring: some old functions neeed to use old to connect with database (without ConnectionManager class), so we need
		'to be sure that command has an connection
		m_config.m_command.Connection = m_config.m_connectionManager.connection

		v_hashToReturn.Add("data", v_hashData)
		v_hashToReturn.Add("executableCodeArray", v_executableCodeArray)
		'validate login fields: username and password
		v_errorMessage = Me.validateLoginFields()
		If v_errorMessage <> "" Then
			v_executableCode = "this.contextObject.printErrorMessage('" & v_errorMessage & "');"
			v_executableCodeArray.Add(v_executableCode)
			Return v_hashToReturn
		End If

		'AccountPreferences.CreateFieldUseExcelOutputIfNonExistent(m_config.m_command)	' TODO: i dont know why this

		'validade account: check if it is blocked
		'v_errorMessage = Me.accountIsBlocked()
		'If (v_errorMessage <> "") Then
		'v_executableCode = "this.contextObject.printErrorMessage('" & v_errorMessage & "');"
		'v_executableCodeArray.Add(v_executableCode)
		'Return v_hashToReturn
		'End If

		Dim innerHTML As String = ""
		Dim innerJavaScript As String = ""

		Dim userFullName As String = ""
		Dim v_expiredPassword As Boolean = False
		Dim v_lastLoginTime As Date


		Dim v_dataReader As OdbcDataReader


		Dim v_autenticate As String = "AND u.password=sha('" & m_config.m_password.Replace("'", "\'") & "');"

		qrySELECT = _
		"SELECT u.fullname, " & _
		"u.last_login_time " & _
		"FROM " & m_config.m_userDB & ".users AS u " & _
		"WHERE u.username='" & m_config.m_username & "' " & v_autenticate

		m_config.m_command.CommandText = qrySELECT
		v_dataReader = m_config.m_connectionManager.executeReader(m_config.m_command)

		If v_dataReader.Read() Then	'user was found, so validate ldap and get some information

			'validating full name
			If Not IsDBNull(v_dataReader("fullname")) Then
				userFullName = v_dataReader("fullname")
			End If

			'validating last login time
			If IsDBNull(v_dataReader("last_login_time")) Then
				v_lastLoginTime = Date.Now()
			Else
				v_lastLoginTime = v_dataReader("last_login_time")
			End If

			v_dataReader.Close()
			Me.updatePostSuccessLoginAttempt()

		Else 'user wasn't found in DB
			v_errorMessage = " Usuário e senha incorretos. Se você errar a senha 4 vezes, seu usuário será bloqueado. "
			v_executableCode = "this.contextObject.printErrorMessage('" & v_errorMessage & "');"
			v_executableCodeArray.Add(v_executableCode)

			v_dataReader.Close()
			'Me.updatePostFailLoginAttempt()

			Return v_hashToReturn
		End If

		' include in executable code array function to set cookies
		v_cookies = Users.getLoginCookies(m_config.m_username, m_config.m_command)

		v_executableCode &= "function myInnerHTMLJavaScript()" & Chr(10)
		v_executableCode &= "{" & Chr(10)

		'v_executableCode &= "DeleteCookie (""" & vCookie & """);" & Chr(10)
		v_executableCode &= "var expdate = new Date (); var neverExpDate = new Date(expdate.getTime() + 10 * 365 * 24 * 60 * 60 * 1000); " & Chr(10)

		'-- 2011-05 André Teixeira: adding the stay loged in option
		If (m_config.m_rememberMe) Then
			'if the user chose to stay signed in we set the cookie expiration to 15 days
			v_executableCode &= "expdate.setTime (expdate.getTime() + (15 * 24 * 60 * 60 * 1000));" & Chr(10)
			v_executableCode &= "SetCookie ('STAY_SIGNED','True',neverExpDate);" & Chr(10)
		Else
			'otherwise we set it to the amount of minutes defined at the project global (normally 15 hours)
			v_executableCode &= "expdate.setTime (expdate.getTime() + (" & HttpContext.Current.Application("logincookieexpires").ToString & " * 60 * 1000));" & Chr(10)
			v_executableCode &= "SetCookie ('STAY_SIGNED','False',neverExpDate);" & Chr(10)
		End If

		'----------------------------- Server code only ------------------------------
		For Each t_entry As DictionaryEntry In v_cookies
			v_executableCode &= "SetCookie('" & t_entry.Key & "','" & t_entry.Value & "',expdate);" & Chr(10)
		Next

		v_executableCode &= "SetCookie ('NETCHART_LANGUAGE','" & HttpContext.Current.Application("NetchartLanguage") & "',expdate);" & Chr(10)
		v_executableCode &= "SetCookie ('EXPIRED_PASSWORD','" & v_expiredPassword & "',expdate);" & Chr(10)
		v_executableCode &= "SetCookie('BROWSERINFO', navigator.appName + ' ' + navigator.appCodeName + ' ' + navigator.appVersion.replace(';','','g') + ' ' + navigator.oscpu,expdate);" & Chr(10)

		'----------------------------- End of Server code only ------------------------------
		v_executableCode &= "}" & Chr(10)
		v_executableCodeArray.Add(v_executableCode)
		v_executableCode = "myInnerHTMLJavaScript();" & Chr(10)
		v_executableCodeArray.Add(v_executableCode)
		v_executableCode = "this.contextObject.printSuccessMessage('" & userFullName & " logged into MLOP.');"
		v_executableCodeArray.Add(v_executableCode)
		v_executableCode = "this.contextObject.loginRequestCallBack(data);" & Chr(10)
		v_executableCodeArray.Add(v_executableCode)

		Return v_hashToReturn
	End Function

	''' <summary>
	''' Sends a new random password to the given user
	''' </summary>
	''' <returns>Hashtable with executable code array and data</returns>
	''' <remarks>
	''' Created by Andre Teixeira on 2011-11-07 (Mantis issue: 01392)
	''' Code Refactored by Fidelis on 2014-12-29
	''' </remarks>
	Public Function forgotPassword() As Hashtable

		Dim v_hashToReturn As Hashtable = New Hashtable
		Dim v_hashData As Hashtable = New Hashtable
		Dim v_executableCodeArray As ArrayList = New ArrayList

		Dim v_errorMessage As String = ""
		Dim v_randomPassword As String = "" 'GetRandomString(8)	'new random password
		Dim v_userMail As String
		Dim v_dataReader As OdbcDataReader

		v_hashToReturn.Add("data", v_hashData)
		v_hashToReturn.Add("executableCodeArray", v_executableCodeArray)

		If HttpContext.Current.Application("useLDAPLogin") Then
			v_errorMessage = "Si prega di contattare l'amministratore LDAP per recuperare la password dell'account."
			v_executableCodeArray.Add("this.contextObject.printErrorMessage('" & v_errorMessage & "');")
			Return v_hashToReturn
		End If

		'validating username 
		If Me.m_config.m_username = "" Then
			v_errorMessage = " Missing fields!"
		ElseIf Len(Me.m_config.m_username) > 20 Then
			v_errorMessage = " User name too long (max 20 characters)"
		End If
		If v_errorMessage <> "" Then
			v_executableCodeArray.Add("this.contextObject.printErrorMessage('" & v_errorMessage & "');")
			Return v_hashToReturn
		End If

		'check if the user exists at the database
		m_config.m_command.CommandText = "SELECT u.email " & _
		"FROM " & m_config.m_userDB & ".users AS u " & _
		"WHERE u.username='" & m_config.m_username & "' "

		v_dataReader = m_config.m_connectionManager.executeReader(m_config.m_command)

		If (v_dataReader.Read) Then
			v_userMail = v_dataReader("email").ToString
			v_dataReader.Close()
		Else
			v_dataReader.Close()
			updateStatisticsTable(LoginResult.invalid_user)
			v_errorMessage = " Could not find this username in our database."
			v_executableCodeArray.Add("this.contextObject.printErrorMessage('" & v_errorMessage & "');")
			Return v_hashToReturn
		End If


		'create new password in db
		m_config.m_command.CommandText = _
			"UPDATE " & m_config.m_userDB & ".users " & _
			"SET password=sha('" & v_randomPassword & "'), failed_attempts = 0" & _
			" WHERE username='" & m_config.m_username & "';"
		m_config.m_connectionManager.executeNonQuery(m_config.m_command)

		'try to send email 
		Try
			Dim v_mail As New MailMessage()

			v_mail.From = New MailAddress(HttpContext.Current.Application("smtpUser"))
			v_mail.Priority = MailPriority.High
			v_mail.To.Add(v_userMail)
			v_mail.ReplyTo = New MailAddress("support@remopt.com")

			v_mail.Priority = MailPriority.High

			v_mail.Subject = "NetChart password recovery"
			v_mail.Body = "Hello," & vbNewLine & vbNewLine & _
			"You requested a password change on NetChart. Your account information is:" & vbNewLine & _
			"username: " & m_config.m_username & vbNewLine & _
			"password: " & v_randomPassword & vbNewLine & vbNewLine & _
			"Please contact us if you didn't requested this change." & vbNewLine & vbNewLine & _
			"Best Regards "

			'EmailManager.sendEmail(v_mail)
			v_hashData.Add("sendedEmail", True)
		Catch ex As Exception
			v_hashData.Add("sendedEmail", False)
			v_hashData.Add("emailMessage", ex.Message)
		End Try

		updateStatisticsTable(LoginResult.new_pass)
		v_executableCodeArray.Add("this.contextObject.forgotPasswordCallBack(data);")

		Return v_hashToReturn
	End Function

	Public Function isThereSomeUserLogged() As Hashtable

		Dim v_hashToReturn As Hashtable = New Hashtable
		Dim v_hashData As Hashtable = New Hashtable
		Dim v_executableCodeArray As ArrayList = New ArrayList

		v_hashToReturn.Add("data", v_hashData)
		v_hashToReturn.Add("executableCodeArray", v_executableCodeArray)

		v_executableCodeArray.Add("this.contextObject.isThereSomeUserLoggedCallBack(data);")

		'Fidelis on 2015-01-12: this is the old way to validate if there is an user logged in, i preserv this
		Dim v_infoArray() As String = getEncriptedInfoFromCookie()
		Dim v_username As String = Trim(v_infoArray(0))

		'if username ="" -> isThereSomeUserLogged = false
		v_hashData.Add("isThereSomeUserLogged", Not v_username = "")
		
		Return v_hashToReturn
	End Function

	

End Class


Public Class LoginConfiguration

	Public m_connectionManager As ConnectionManager
	Public m_userDB As String
	Public m_username As String
	Public m_password As String
	Public m_newPassword1 As String
	Public m_newPassword2 As String
	Public m_rememberMe As Boolean
	Public m_command As OdbcCommand


	Public Function runFromInterface(ByVal p_context As Web.HttpRequest) As Hashtable

		Dim v_model As Hashtable = New Hashtable
		m_userDB = HttpContext.Current.Application("DBName")
		m_command = New OdbcCommand()
		m_connectionManager = New ConnectionManager()

		Dim v_login As Login = New Login(Me)

		Using m_connectionManager
			'select action
			Select Case p_context("action")
				Case "login"
					Me.getLoginDataFromRequest(p_context)
					v_model = v_login.login()
				Case "forgotPassword"
					Me.getForgorPasswordDataFromRequest(p_context)
					v_model = v_login.forgotPassword()
				Case "isThereSomeUserLogged"
					v_model = v_login.isThereSomeUserLogged()
			End Select

		End Using

		Return v_model

	End Function

	Private Sub getLoginDataFromRequest(ByVal p_context As Web.HttpRequest)
		'get data from request

		If (Not p_context("u") Is Nothing) Then
			Me.m_username = p_context("u")
		End If

		If (Not p_context("p") Is Nothing) Then
			Me.m_password = DecriptString(Me.m_username, p_context("p"))
		End If

		If (Not p_context("rememberMe") Is Nothing) Then
			Me.m_rememberMe = p_context("rememberMe")
		End If

	End Sub

	Private Sub getExpiredPasswordDataFromRequest(ByVal p_context As Web.HttpRequest)
		'get data from request

		If (Not p_context("u") Is Nothing) Then
			Me.m_username = p_context("u")
		End If

		If (Not p_context("pOld") Is Nothing) Then
			Me.m_password = DecriptString(Me.m_username, p_context("pOld"))
		End If

		If (Not p_context("pNew1") Is Nothing) Then
			Me.m_newPassword1 = DecriptString(Me.m_username, p_context("pNew1"))
		End If

		If (Not p_context("pNew2") Is Nothing) Then
			Me.m_newPassword2 = DecriptString(Me.m_username, p_context("pNew2"))
		End If

		If (Not p_context("rememberMe") Is Nothing) Then
			Me.m_rememberMe = p_context("rememberMe")
		End If

	End Sub

	Private Sub getForgorPasswordDataFromRequest(ByVal p_context As Web.HttpRequest)
		'get data from request
		If (Not p_context("u") Is Nothing) Then
			Me.m_username = p_context("u")
		End If

	End Sub

End Class