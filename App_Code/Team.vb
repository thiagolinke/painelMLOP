Imports Microsoft.VisualBasic
Imports System.Data.Odbc
Imports System.Collections.Generic
Imports Util

Public Class Team

	Shared Function GetContents() As Hashtable

		Dim v_model As Hashtable = New Hashtable()
		Dim v_codeArray As List(Of String) = New List(Of String)
		Dim v_data As Hashtable = New Hashtable()
		v_model.Add("data", v_data)
		v_model.Add("executableCodeArray", v_codeArray)

		'-----CONNECT TO DB-----
		Dim v_connection As OdbcConnection
		Dim v_command As OdbcCommand
		Dim v_dataReader As OdbcDataReader

		connectDB(v_connection, v_command)
		'-----END OF CONNECT TO DB-----

		Dim v_currentTeam As String = "PSG"

		v_command.CommandText = "SELECT `ID`,`pri_position`,`name`,`age`,`salary`,(`salary`*20) " & _
														"FROM `" & HttpContext.Current.Application("player_table") & "` " & _
														"WHERE `team` = '" & v_currentTeam & "' " & _
														"UNION SELECT `ID`,`pri_position`,`name`,`age`,`salary`,(`salary`*20) " & _
														"FROM `" & HttpContext.Current.Application("gk_table") & "` " & _
														"WHERE `team` = '" & v_currentTeam & "' "
		v_dataReader = v_command.ExecuteReader()

		v_data.Add("players", New List(Of String()))
		Dim v_line() As String
		While v_dataReader.Read()
			v_line = New String() {v_dataReader(0), v_dataReader(1), v_dataReader(2), v_dataReader(3), v_dataReader(4), v_dataReader(5)}
			v_data("players").Add(v_line)
		End While


		v_codeArray.Add("this.contextObject.setPlayers(data.players);")
		Return v_model

	End Function

	Shared Function GetPlayerStats(ByVal p_playerID As String) As Hashtable

		Dim v_model As Hashtable = New Hashtable()
		Dim v_codeArray As List(Of String) = New List(Of String)
		Dim v_data As Hashtable = New Hashtable()
		v_model.Add("data", v_data)
		v_model.Add("executableCodeArray", v_codeArray)

		'-----CONNECT TO DB-----
		Dim v_connection As OdbcConnection
		Dim v_command As OdbcCommand
		Dim v_dataReader As OdbcDataReader

		connectDB(v_connection, v_command)
		'-----END OF CONNECT TO DB-----

		v_command.CommandText = "SELECT * FROM `" & HttpContext.Current.Application("player_table") & "` WHERE `ID`='" & p_playerID & "';"
		v_dataReader = v_command.ExecuteReader()

		If Not v_dataReader.HasRows Then
			v_dataReader.Close()

			v_command.CommandText = "SELECT * FROM `" & HttpContext.Current.Application("gk_table") & "` WHERE `ID`='" & p_playerID & "';"
			v_dataReader = v_command.ExecuteReader()
		End If

		v_dataReader.Read()
		v_data.Add("stats", New List(Of String))
		v_data.Add("ID", v_dataReader("ID").ToString())
		v_data("stats").Add(v_dataReader("name").ToString())
		v_data("stats").Add(v_dataReader("team").ToString())
		For i As Integer = 4 To v_dataReader.FieldCount - 1
			v_data("stats").Add(v_dataReader(i).ToString())
		Next i
		v_dataReader.Close()

		v_codeArray.Add("this.contextObject.setPlayerStats(data.ID, data.stats)")
		Return v_model

	End Function


End Class
