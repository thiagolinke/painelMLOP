<%@ WebHandler Language="VB" Class="teamRequestHandler" %>

Imports System
Imports System.Web
Imports System.Web.Script.Serialization
Imports Team

Public Class teamRequestHandler : Implements IHttpHandler
    
	Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
	
		'v_model must have 2 childs:
		'data: an object with any data that will be sent to the client
		'executableCodeArray: an array with the javascript commands to execute at the client
		Dim v_model As Hashtable
		context.Response.ContentType = "text/plain"
		
		
		Try

			'Dim v_userName As String = validateuser(True, True)
			'If (v_userName = "") Then
			'Exit Sub
			'End If

			' dispatch processing regarding to type of request
			Select Case context.Request("f")
				
				Case "get_contents"
					v_model = Team.GetContents()
					
				Case "get_player_stats"
					Dim v_id As String
					v_id = context.Request("playerID")
					v_model = Team.GetPlayerStats(v_id)
			
				Case Else
					Throw New Exception("Function not recognized!")
			End Select
			
		Catch ex As Exception
			v_model = New Hashtable()
			v_model.Add("data", ex.Message)
			v_model.Add("executableCodeArray", _
					New String() {"this.contextObject.printErrorMessage(data);"})
		End Try

		' !!! According to w3c recommendation all data of a JSON message must be wrapped in a "d" property
		' to make the code compliant to WWW standards and prevents a cross-site scripting attack 
		' from accessing data from AJAX JSON services on other domains.
		Dim t_wrappingMap As New Hashtable()
		t_wrappingMap.Add("d", v_model)

		Dim serializer As New JavaScriptSerializer()
		serializer.MaxJsonLength = Int32.MaxValue
		context.Response.Write(serializer.Serialize(t_wrappingMap))

	End Sub
 
	
	
	
	Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
		Get
			Return False
		End Get
	End Property

End Class