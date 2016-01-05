Imports Microsoft.VisualBasic
Imports System.Data.Odbc
Imports System.Collections.Generic
Imports System.Data

''' <summary>
''' Responsable to manager the connections with the database, well as execute commands and handle errors.
''' </summary>
''' <remarks>Created by Lucas Gustavo on 2013-08-09</remarks>
Public Class ConnectionManager
  Implements IDisposable

  Protected disposed As Boolean = False
  Shared m_lockObject As Object = New Object
  Public connection As OdbcConnection
  Private m_tech As String
  Private Const m_attemptsLimit As Integer = 3
	Private m_attemptsDone As Integer
	Private m_closeConnection As Boolean = True

	Public Sub New()
		createConnection()
	End Sub

#Region " IDisposable Support "
  'Interface implemented to ensure that the connections will be closed
  Public Overloads Sub Dispose() Implements IDisposable.Dispose
		Dispose(m_closeConnection)
    GC.SuppressFinalize(Me)
	End Sub

	Protected Overrides Sub Finalize()
		'We removed the finalize content because it was being called on unespected times (while the object was still being used)
		'on some function (ReportManager) and it was causing connection closed errors
		'Dispose(m_closeConnection)
		'MyBase.Finalize()
	End Sub

	Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
		If (disposing) Then
			If (Not IsNothing(connection) AndAlso connection.State <> Data.ConnectionState.Closed) Then
				Try
					connection.Close()
				Catch ex As Exception
					'Andre Teixeira on 2014-02-28:
					'do nothing, what could we do? :(

					'Had to do this because of an error with download raw counters with a lot of results:
					'Error: Identifier was not initialized.
					Dim t = "stop"
				End Try
			End If
		End If

		Me.disposed = True

	End Sub
#End Region

	''' <summary>
	''' Create and open a new connection
	''' </summary>
	''' <remarks></remarks>
	Private Sub createConnection()
		If (IsNothing(connection)) Then

			connection = New OdbcConnection(getConnectionString())
			connection.Open()

		ElseIf (connection.State = Data.ConnectionState.Closed) Then
			connection.Open()
		End If
	End Sub

	''' <summary>
	''' Get the instance of OdbcConnection.
	''' </summary>
	''' <returns>Instance of OdbcConnection</returns>
	''' <remarks>This is a temporary method. It had create because we've some functions that need a OdbcConnection to work fine. Then, avoid using it! </remarks>
	Public Function getConnectionInstance() As OdbcConnection
		Return connection
	End Function


	''' <summary>
	''' Create a new command using the correct connection
	''' </summary>
	''' <returns>A new command</returns>
	''' <remarks></remarks>
	Public Function createCommand() As OdbcCommand
		Dim v_command As OdbcCommand
		v_command = New OdbcCommand
		v_command.Connection = connection

		Return v_command
	End Function

  ''' <summary>
  ''' Executes an SQL statement and returns the number of rows affected
  ''' </summary>
  ''' <param name="p_command">The command with the SQL instruction</param>
  ''' <returns>The number of rows affected</returns>
  ''' <remarks></remarks>
  Public Function executeNonQuery(ByVal p_command As OdbcCommand) As Integer
    'Using connection
    Try
			checkConnectionState()

      p_command.Connection = connection
      executeNonQuery = p_command.ExecuteNonQuery()

      m_attemptsDone = 0
    Catch ex As OdbcException

      'To avoid infinite loops, we limit the error handling to not execute more than 3 times 
      If (m_attemptsDone < m_attemptsLimit AndAlso handlingException(ex)) Then
        'if the handlingException method returns true, it means that some erro handling was executed with sucess
        'So, we need execute the query again
				executeNonQuery = executeNonQuery(p_command)
      Else
        Throw ex
      End If
    End Try
    'End Using

  End Function


  ''' <summary>
  ''' Executes an SQL statement and returns the datareader with the results
  ''' </summary>
  ''' <param name="p_command">The command with the SQL instruction</param>
  ''' <returns>The datareader with the results</returns>
  ''' <remarks></remarks>
  Public Function executeReader(ByVal p_command As OdbcCommand) As OdbcDataReader

    Try
			checkConnectionState()

      p_command.Connection = connection
			executeReader = p_command.ExecuteReader()

			m_attemptsDone = 0
    Catch ex As OdbcException
      'To avoid infinite loops, we limit the error handling to not execute more than 3 times 
      If (m_attemptsDone < m_attemptsLimit AndAlso handlingException(ex)) Then
        'if the handlingException method returns true, it means that some erro handling was executed with sucess
        'So, we need execute the query again
				executeReader = executeReader(p_command)
      Else
        Throw ex
      End If

    End Try


  End Function

  ''' <summary>
  ''' Handle errors found during SQL execution
  ''' </summary>
  ''' <param name="ex">Error that occour during application execution</param>
  ''' <returns>Boolean indicating if the error has any treatment</returns>
  ''' <remarks></remarks>
  Private Function handlingException(ByVal ex As OdbcException) As Boolean

    SyncLock m_lockObject
      handlingException = False

      Dim v_command As OdbcCommand = New OdbcCommand
      v_command.Connection = connection

      For Each t_erro As OdbcError In ex.Errors


        If (t_erro.Message.ToString.Contains("is marked as crashed")) Then
          Dim re As Regex = New Regex("Table '(.*)' is marked as crashed")
          Dim mc As MatchCollection = re.Matches(t_erro.Message)
          For Each m As Match In mc
            For groupIdx As Integer = 1 To m.Groups.Count - 1

              Dim t_values As String() = m.Groups(1).Value.Split("\")
              'the table name always will be in the last position of the array
              Dim t_tableName As String = t_values(t_values.Length - 1)
              Dim t_dataBaseName As String = t_values(t_values.Length - 2)

              v_command.CommandText = _
                "REPAIR TABLE " & t_dataBaseName & "." & t_tableName & " QUICK "

              Try
                v_command.ExecuteNonQuery()

                handlingException = True
              Catch err As Exception
                handlingException = False
              End Try

              m_attemptsDone += 1
            Next
					Next

					'Unhandled error
					handlingException = False
				End If

			Next
    End SyncLock

	End Function


	''' <summary>
	''' Define the correct connectionString to use, based on the used tecnology and the type of database (parameter, users, errors)
	''' </summary>
	''' <returns>The connectionString</returns>
	''' <remarks></remarks>
  Private Function getConnectionString() As String
		getConnectionString = HttpContext.Current.Application("connectionString")
	End Function



	Public Sub checkConnectionState()
		' Checks if connection is still open and reopen it if necessary
		If connection.State = ConnectionState.Broken Then
			connection.Close()
			connection.Open()
		ElseIf connection.State = ConnectionState.Closed Then
			connection.Open()
		End If
	End Sub

End Class
