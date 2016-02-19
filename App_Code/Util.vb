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
	Shared Function validateuser(Optional ByVal p_jsonResponse As Boolean = False) As String

		Dim v_infoArray() As String = getEncriptedInfoFromCookie()
		Dim s_username = v_infoArray(0)

		validateuser = Trim(s_username)

		' 2010-01-02 Linke: Force user to login again if username could not be validated
		If validateuser.Equals("") Then
			redirectToLogin(p_jsonResponse)
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
			If Not HttpContext.Current.Request.Cookies("MLOP") Is Nothing Then
				If Not HttpContext.Current.Request.Cookies("MLOP").Values("ASPXVAL") Is Nothing Then
					'read cookie
					vIPhashFinalHH = HttpContext.Current.Request.Cookies("MLOP").Values("ASPXVAL").ToString
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

	''' <summary>
	''' Used to decript String that was encripted using the function EncriptString
	''' </summary>
	''' <param name="p_key">Key user to decript (must be the same used to encript)</param>
	''' <param name="p_toDecript">Information to be decripted</param>
	''' <returns>A new descripted string</returns>
	''' <remarks>Refactored by Andre Teixeira on 2014-04-11</remarks>
	Shared Function DecriptString(ByVal p_key As String, ByVal p_toDecript As String) As String

		Dim v_encriptationSequence As Integer() = {1, 3, 1, 9, 3, 2, 8, 2, 2, 3}
		Dim v_paddingSequence As String() = {"A", "d", "X", "E", "Y", "E", "b", "a", "l", "o"}

		Dim v_paddingSequenceSize As Integer = v_paddingSequence.Length
		Dim v_encriptationSequenceSize As Integer = v_encriptationSequence.Length
		Dim v_keyLength As Integer = p_key.Length

		Dim v_wordSize As Integer = 5
		Dim i As Integer = 0

		Dim v_toReturn As String = ""

		Dim v_numWords As Integer = p_toDecript.Length / v_wordSize

		For i = 0 To v_numWords - 1

			Dim v_tempString As String = p_toDecript.Substring(i * v_wordSize, v_wordSize)
			v_tempString = v_tempString.TrimEnd(v_paddingSequence(i Mod v_paddingSequenceSize))

			v_tempString = CInt(v_tempString) - (v_encriptationSequence(i Mod v_encriptationSequenceSize) * Asc(p_key(i Mod v_keyLength)))
			v_toReturn &= Chr(CInt(v_tempString))

		Next

		If v_toReturn = "all" Then
			v_toReturn = HttpContext.Current.Application("techsList")
		End If

		Return v_toReturn

	End Function

	Public Shared Sub redirectToLogin(ByVal p_jsonResponse As Boolean)

		Dim v_response As String
		v_response = "alert('Sua sessão está expirada. Você será redirecionado para a página de login.'); document.cookie = 'MLOP=;expires=' + new Date().toGMTString();" & _
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


	''' <summary>
	''' Function removes the specified last characters from the string
	''' </summary>
	''' <param name="p_Str">String where last character needs to be removed</param>
	''' <param name="p_amount">The amount of caracters to be removed</param>
	''' <remarks>
	''' If the specified amount is greater than string lenght	p_Str will become 
	''' System.String.Empty that is the same as ""
	''' 
	''' Consider to don't add last char also consider to use StringBuilder instead of String:
	''' <code>
	''' For t_i As Integer = 0 To v_someArray.count - 1
	'''   If (t_i <> 0) Then
	'''     v_resultStringBuilder.Append(",")
	'''   End If
	''' 
	'''   v_resultStringBuilder.Append(v_someArray(t_i))
	'''          
	''' Next
	''' </code>
	''' </remarks>
	''' Modified by André Teixeira in 2010-11-25 to add the
	''' p_amount optional value	
	Shared Sub RemoveLastChar(ByRef p_Str As String, Optional ByVal p_amount As Integer = 1)
		If (p_Str.Length < p_amount) Then
			p_amount = p_Str.Length
		End If
		p_Str = p_Str.Substring(0, p_Str.Length - p_amount)
	End Sub

	Shared Sub RemoveLastChar(ByRef p_Str As StringBuilder, Optional ByVal p_amount As Integer = 1)
		If (p_Str.Length < p_amount) Then
			p_amount = p_Str.Length
		End If
		p_Str.Remove(p_Str.Length - p_amount, p_amount)
	End Sub


	Public Shared Function GetPublicAdress(ByVal p_toolUsed As String) As String
		Dim v_url As String

		Select Case p_toolUsed.ToLower
			Case "porta_ecuador"
				v_url = "netchart.remopt.com/netchart_porta/"
		End Select

		v_url = "http://" & v_url

		Return v_url
	End Function


	''' <summary>
	''' Used to encript a given string using the given key
	''' </summary>
	''' <param name="p_key">Key user to encript (must be the same used to decript)</param>
	''' <param name="p_toEncript">Information to be encripted</param>
	''' <returns>A new encripted string that can be decripted using the method DecriptString of this same class</returns>
	''' <remarks>Refactored by Andre Teixeira on 2014-04-11
	''' 
	''' The function uses a symmetric-key block cipher algorithm
	''' </remarks>
	Shared Function EncriptString(ByVal p_key As String, ByVal p_toEncript As String) As String

		Dim v_encriptationSequence As Integer() = {1, 3, 1, 9, 3, 2, 8, 2, 2, 3}
		Dim v_paddingSequence As String() = {"A", "d", "X", "E", "Y", "E", "b", "a", "l", "o"}

		Dim v_paddingSequenceSize As Integer = v_paddingSequence.Length
		Dim v_encriptationSequenceSize As Integer = v_encriptationSequence.Length
		Dim v_keyLength As Integer = p_key.Length

		Dim v_wordSize As Integer = 5
		Dim v_toReturn As String = ""

		For i As Integer = 0 To p_toEncript.Length - 1

			Dim v_tempString As String = Asc(p_toEncript(i)) + (v_encriptationSequence(i Mod v_encriptationSequenceSize) * Asc(p_key(i Mod v_keyLength)))
			v_tempString = v_tempString.PadRight(v_wordSize, v_paddingSequence(i Mod v_paddingSequenceSize))
			v_toReturn &= v_tempString

		Next

		Return v_toReturn
	End Function


	''' <summary>
	''' Given a username, generates the encrypted data to be stored in the cookie
	''' The encrypted data is basically a huge encrypted string containing username, ip adress and cookie expiration date.
	''' </summary>
	''' <param name="username">The username whose information will be encrypted</param>
	''' <returns>Returns a huge encrypted string containing username,ip adress and cookie expiration date.</returns>
	''' <remarks></remarks>
	Shared Function getcookievalue(ByVal username As String) As String

		Dim username20 As String = ""
		username20 = Space(20)

		Dim vIPaddress As String
		Dim vIPhash As String = ""
		Dim vIPhashFull As String = ""
		Dim vIPhashFinal As String = ""
		Dim vIPhashFinalHH As String = ""
		Dim vIPhashFinalHex As String = ""
		Dim vHex As String = ""
		Dim i As Integer

		Dim lowerbound As Integer = 65
		Dim upperbound As Integer = 90
		Dim vInit As Integer

		Dim mycookiedate As DateTime
		Dim s_mycookiedate As String

		If HttpContext.Current.Application("logincookieexpires") <> 0 Then
			mycookiedate = DateAdd(DateInterval.Minute, HttpContext.Current.Application("logincookieexpires"), Now())
		Else
			mycookiedate = DateAdd(DateInterval.Minute, 60, Now()) '60 minutes
		End If
		s_mycookiedate = Format(mycookiedate, "yyyyMMddHHmmss")


		Randomize()
		vInit = Int((9 - 1 + 1) * Rnd() + 1)


		If Len(username) >= 20 Then
			username20 = Left(username, 20)
		Else
			username20 = username & Space(20 - Len(username))
		End If

		Dim v_ip As String

		Try
			v_ip = HttpContext.Current.Request.UserHostAddress
		Catch ex As Exception
			'When doing a request from a thread at scheduler UserHostAddress call will generate an error
			v_ip = ""
		End Try

		vIPaddress = username20 & s_mycookiedate & v_ip

		'code user host IP address
		If Len(vIPaddress) > 0 Then
			For i = 1 To Len(vIPaddress)
				vIPhash = vIPhash & Format((Asc(Mid(vIPaddress, i, 1)) + i + vInit), "000")
			Next i
		End If
		If Len(vIPhash) > 0 Then
			For i = 1 To Len(vIPhash)
				vIPhashFull = vIPhashFull & Mid(vIPhash, i, 1) & Chr(Int((upperbound - lowerbound + 1) * Rnd() + lowerbound))
			Next i
		End If
		vIPhashFull = vIPhashFull & vInit.ToString
		vIPhashFinal = ""
		For i = 1 To Len(vIPhashFull)
			vIPhashFinal = vIPhashFinal & Chr(Asc(Mid(vIPhashFull, i, 1)) + vInit)
		Next i
		vIPhashFinal = vIPhashFinal & vInit.ToString
		vIPhashFinalHH = ""
		For i = 1 To Len(vIPhashFinal)
			vIPhashFinalHH = vIPhashFinalHH & Format((Asc(Mid(vIPhashFinal, i, 1)) + 348), "000")
		Next i

		Dim v_append As String = ""
		Dim v_appId = ""
		If Len(HttpContext.Current.Application("userRole")) >= 20 Then
			v_appId = Left(HttpContext.Current.Application("userRole"), 20)
		Else
			v_appId = HttpContext.Current.Application("userRole") & Space(20 - Len(HttpContext.Current.Application("userRole")))
		End If

		Dim v_aux As String = ""
		For j As Integer = 0 To v_appId.Length - 1
			v_aux &= Asc(v_appId(j))
			Select Case v_aux.Length
				Case 1
					v_aux = "00" & v_aux
				Case 2
					v_aux = "0" & v_aux
				Case 3
					v_aux = v_aux
			End Select
			v_append &= v_aux & ((v_aux(0) + v_aux(1) + v_aux(2) + Asc(v_aux(1))) Mod 10)
			v_aux = ""
		Next


		getcookievalue = v_append & vIPhashFinalHH

	End Function

End Class
