<%@ WebHandler Language="VB" Class="LoginHandler" %>

Imports System
Imports System.Web
Imports System.Web.Script.Serialization

Public Class LoginHandler : Implements IHttpHandler
    ''' <summary>
    ''' 	 This method handler
    ''' </summary>
    ''' <param name="context"></param>
    ''' <remarks></remarks>
    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest

        'v_model must have 2 childs:
        'data: an object with any data that will be sent to the client
        'executableCodeArray: an array with the javascript commands to execute at the client
        Dim v_model As Hashtable
        context.Response.ContentType = "text/plain"
        Dim v_loginConfig As LoginConfiguration = New LoginConfiguration()
        Try
            'dispatch processing regarding to type of request			
            v_model = v_loginConfig.runFromInterface(context.Request)

        Catch ex As Exception
            Dim v_errorDialogHtml As String = "Error: " & ex.Message
            ' v_errorDialogHtml = EmailManager.sndMail_ReturningErrInterfaceHtmlException(ex, "")

            v_model = New Hashtable()
            v_model.Add("data", v_errorDialogHtml)
            v_model.Add("executableCodeArray",
                    New String() {"this.parent.printErrorMessage(data);"})

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