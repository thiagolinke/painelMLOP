Imports Microsoft.VisualBasic
Imports System.Data.Odbc

Public Class Util

	Shared Sub connectDB(ByRef p_connection As OdbcConnection, ByRef p_command As OdbcCommand)
		p_connection = New OdbcConnection(HttpContext.Current.Application("connectionString"))
		p_command = New OdbcCommand()
		p_connection.Open()
		p_command.Connection = p_connection
	End Sub

	''' <summary>
	''' This function validates the user.
	''' It calls the getEncriptedInfoFromCookie to get username, user ip and cookie expiration date.
	''' If the date is still ok (not expired),
	''' the function querys the database in order to get the user role.
	''' If the user role is OK (the same as in the HttpContext.Current.Application('userRole'),
	''' it validates the user
	''' </summary>
	''' <param name="p_redirect">OPTIONAL: If TRUE, redirects the user to the login page if username could not be validated. If missing, default value is TRUE.</param>
	''' <returns>  
	''' Returns username if the user was successfully validated
	''' Returns empty string if the user wasnt successfully validated
	''' Redirects user to login page if user was not successfully validated and optional argument p_redirect set as TRUE</returns>
	''' <remarks>
	''' Modified by Linke - 2011-03-25 - Mantis Issue 0001059
	''' </remarks>
	Shared Function validateuser(Optional ByVal p_redirect As Boolean = True, Optional ByVal p_jsonResponse As Boolean = False) As String
		'----- If validateUser flag is set to false skip validation -----
		If (Not IsNothing(HttpContext.Current.Request("crontab"))) Then
			Return "crontab"
		End If

		Dim v_validateUser As String = HttpContext.Current.Request("validateUser")
		If v_validateUser Is Nothing Then
			v_validateUser = "true"
		End If
		If v_validateUser.Equals("false") Then
			Return ""
			Exit Function
		End If
		'----- END OF If validateUser flag is set to false skip validation -----

		Dim v_infoArray() As String = getEncriptedInfoFromCookie()
		Dim s_username = v_infoArray(0)

		validateuser = Trim(s_username)

		' 2010-01-02 Linke: Force user to login again if username could not be validated
		If p_redirect Then
			If validateuser.Equals("") Then
				redirectToLogin(p_jsonResponse)
			End If
		End If

	End Function

	''' <summary>
	''' This function runs the algorithm to decrypt the information stored in the user cookie
	''' </summary>
	''' <returns>Returns an array [username,ipaddress,cookiedate]</returns>
	''' <remarks>
	''' Created by Marcelo Negri - 2010-April-09 - Mantis Issue 0000439
	''' </remarks>
	Shared Function getEncriptedInfoFromCookie() As String()

		Dim v_returnArray(2) As String
		v_returnArray(0) = ""
		v_returnArray(1) = ""
		v_returnArray(2) = ""
		Try
			Dim vIPhashFinalHH As String = ""
			If Not HttpContext.Current.Request.Cookies("ASPXREMOPT") Is Nothing Then
				If Not HttpContext.Current.Request.Cookies("ASPXREMOPT").Values("ASPXVAL") Is Nothing Then
					'read cookie
					vIPhashFinalHH = HttpContext.Current.Request.Cookies("ASPXREMOPT").Values("ASPXVAL").ToString
				End If
			End If

			If vIPhashFinalHH.Length < 80 Then
				Return v_returnArray
			End If

			Dim v_appId As String = vIPhashFinalHH.Substring(0, 80)
			vIPhashFinalHH = vIPhashFinalHH.Substring(80)

			Dim v_aux As String = ""
			For k As Integer = 0 To v_appId.Length - 1 Step 4
				Dim v_tempChar As String = v_appId.Substring(k, 3)
				Dim v_tempInt As Integer = 100
				If IsNumeric(v_tempChar) Then
					v_tempInt = CInt(v_tempChar)
				Else
					v_tempInt = 100
				End If
				v_aux &= Chr(v_tempInt)
			Next

			If HttpContext.Current.Application("userRole") <> Trim(v_aux) Then
				Return v_returnArray
			End If

			Dim vIPhashFinal As String = ""
			Dim vIPhashFull As String = ""
			Dim vInit As Integer = 0
			Dim vIPhash As String = ""
			Dim vIPaddress As String = ""
			Dim vIPaddressGet As String = ""
			Dim s_mycookiedate As String = ""
			Dim s_username As String = ""
			Dim i, j As Integer
			If vIPhashFinalHH <> "" Then
				'decode

				vIPhashFinal = ""
				For i = 1 To Len(vIPhashFinalHH) Step 3
					vIPhashFinal = vIPhashFinal & Chr(Val(Mid(vIPhashFinalHH, i, 3)) - 348)
				Next i
				vIPhashFull = ""
				vInit = Val(Right(vIPhashFinal, 1))
				vIPhashFinal = Left(vIPhashFinal, Len(vIPhashFinal) - 1)
				For i = 1 To Len(vIPhashFinal)
					vIPhashFull = vIPhashFull & Chr(Asc(Mid(vIPhashFinal, i, 1)) - vInit)
				Next i
				vInit = Val(Right(vIPhashFull, 1))
				vIPhashFull = Left(vIPhashFull, Len(vIPhashFull) - 1)
				vIPhash = ""
				For i = 1 To Len(vIPhashFull)
					If i Mod 2 <> 0 Then
						vIPhash = vIPhash & Mid(vIPhashFull, i, 1)
					End If
				Next i
				vIPaddress = ""
				j = 1
				For i = 1 To Len(vIPhash) Step 3
					vIPaddress = vIPaddress & Chr(Val(Mid(vIPhash, i, 3)) - j - vInit)
					j = j + 1
				Next i
			End If


			If Len(vIPaddress) > 20 Then
				s_username = Left(vIPaddress, 20)
				vIPaddress = Right(vIPaddress, Len(vIPaddress) - 20)
			End If
			If Len(vIPaddress) > 14 Then
				s_mycookiedate = Left(vIPaddress, 14)
				vIPaddress = Right(vIPaddress, Len(vIPaddress) - 14)
			End If

			v_returnArray(0) = s_username.Trim()
			v_returnArray(1) = vIPaddress.Trim()
			v_returnArray(2) = s_mycookiedate.Trim()

		Finally
			getEncriptedInfoFromCookie = v_returnArray
		End Try

	End Function


	Public Shared Sub redirectToLogin(ByVal p_jsonResponse As Boolean)
		Dim v_response As String
		v_response = "alert('Your current login session is expired You will be redirected to the login page.'); document.cookie = 'ASPXREMOPT=;expires=' + new Date().toGMTString();" & _
				" eval('window.open(""login.html?f=login&message=cookie"",""_self"");');"

		If (Not p_jsonResponse) Then
			HttpContext.Current.Response.Write("<!-- " & v_response & " -->")
			HttpContext.Current.Response.Write("<script> " & v_response & " </script>")
			HttpContext.Current.ApplicationInstance.CompleteRequest()
		Else
			Dim v_model = New Hashtable()
			v_model.Add("executableCodeArray", _
							New String() {v_response})

			Dim t_wrappingMap As New Hashtable()
			t_wrappingMap.Add("d", v_model)

			Dim serializer As New System.Web.Script.Serialization.JavaScriptSerializer()
			HttpContext.Current.Response.Write(serializer.Serialize(t_wrappingMap))
		End If
	End Sub
End Class
