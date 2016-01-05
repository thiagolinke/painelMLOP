Imports Util

Partial Class painel
    Inherits System.Web.UI.Page

	Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

		'TEST IF APPLICATION IS RUNNING
		'-------------------------------------------
		If Application("status") = "stopped" Then
			Server.Transfer("coffeebreak.aspx")
		End If

		Dim v_userName As String = validateuser()

		If v_userName = "" Then
			Exit Sub
		End If


	End Sub


End Class
