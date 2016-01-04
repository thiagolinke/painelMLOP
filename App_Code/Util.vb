Imports Microsoft.VisualBasic
Imports System.Data.Odbc

Public Class Util

	Shared Sub connectDB(ByRef p_connection As OdbcConnection, ByRef p_command As OdbcCommand)
		p_connection = New OdbcConnection(HttpContext.Current.Application("connectionString"))
		p_command = New OdbcCommand()
		p_connection.Open()
		p_command.Connection = p_connection
	End Sub

End Class
