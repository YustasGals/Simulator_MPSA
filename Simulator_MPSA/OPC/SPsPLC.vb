Option Explicit On


' !!! режим запись в контроллер и запись в архив
' !!! синхронизация БД уставок
' !!! уставки разных контроллеров/типов связи считывать записывать в параллельном режиме
' !!! Тип связи с контроллером перенести в БД т.к. м.б. много разных контроллеров с разными параметрами связи
' !!! не загружать в контроллер уставки с несоблюдением ограничений

''' <summary>
''' Чтение/запись устройств (контроллера)
''' </summary>
Module SPsPLC
  Public OPCVarsPerCycle As Integer = 100                           ' Максимальное число OPC блоков читаемых за один запрос через OPC
  Public OPCCycleSleepTime As Integer = 50                          ' Таймаут между последовательными чтениями OPC сервера

  Public Enum SPsPLCOperationIO
    WriteChangedCurrentTab = 1                                      ' Запись измененных уставок текущего листа
    WriteCurrentTab = 2                                             ' Запись всех уставок текущего листа
    WriteChanged = 3                                                ' Запись всех измененных уставок
    WriteAll = 4                                                    ' Запись всех уставок
    ReadChangedCurrentTab = 11                                      ' Считывание измененных уставок текущего листа
    ReadCurrentTab = 12                                             ' Считывание всех уставок текущего листа
    ReadChanged = 13                                                ' Считывание всех измененных уставок
    ReadAll = 14                                                    ' Считывание всех уставок
  End Enum

  ''' <summary>
  ''' Открывает OPC сервер
  ''' </summary>
  ''' <return>
  ''' Возвращает true при успешной операции
  ''' </return>
  Public Function AT_OpenOPCServer(ByRef OPCSrvr As Opc.Da.Server, ServerName As String, Optional ReOpen As Boolean = False) As Boolean
    If ReOpen Then AT_CloseOPCServer(OPCSrvr)

    Try
      If OPCSrvr Is Nothing OrElse ReOpen Then
        If ReOpen Then AT_CloseOPCServer(OPCSrvr)
        OPCSrvr = New Opc.Da.Server(New OpcCom.Factory(), New Opc.URL("opcda://localhost/" & ServerName))
        OPCSrvr.Connect()
      End If

      Return True
    Catch ex As Exception
      dbg.AT_DbgMsg("Ошибка поключения к OPC Серверу " & ServerName & ". " & ex.Message, Services.MsgTypes.MsgError)
      AT_CloseOPCServer(OPCSrvr)
      Return False                                                  ' Не удалось инициализировать сервер
    End Try
  End Function

  ''' <summary>
  ''' Закрывает OPC сервер
  ''' </summary>
  Public Sub AT_CloseOPCServer(ByRef OPCSrvr As Opc.Da.Server)
    Try
      If OPCSrvr IsNot Nothing Then
        Try
          OPCSrvr.Disconnect()
        Catch ex As Exception
          dbg.AT_DbgMsg("Ошибка закрытия соединения с OPC сервером. " & ex.Message, Services.MsgTypes.MsgError)
        End Try

        OPCSrvr.Dispose()                                           ' Releases the server (MUST be called to release the underlying COM object).
        OPCSrvr = Nothing
      End If
    Catch ex As Exception
      dbg.AT_DbgMsg("Ошибка освобождения ресурсов OPC сервера. " & ex.Message, Services.MsgTypes.MsgError)
      OPCSrvr = Nothing
    End Try
  End Sub

  ''' <summary>
  ''' Читает/записывает данные в/из ПЛК с использованием протокола OPC
  ''' </summary>
  Public Sub IO_OPCTagsNew(ServerName As String, isWrite As Boolean, ByRef pbar As ToolStripProgressBar, ByRef rngs As Range())
    Dim OPCSrvr As Opc.Da.Server = Nothing, opcItm As Object = Nothing, opcRes As Object
    Dim i As Integer, j As Integer, k As Integer
    Dim LastRng As Integer, LastRow As Integer = 0, LastCell As Integer = 0, EndRng As Integer, CurCell As Integer, CurRow As Integer
    Dim cnt As Integer, indxs As UInteger()
    Dim pStep As Double, pCur As Double = 0

    AT_OpenOPCServer(OPCSrvr, ServerName)                           ' Инициализируем OPC сервер
    If OPCSrvr Is Nothing Then Return                               ' Не удалось инициализировать сервер

    ' Подчитываем кол-во переменных для операции для вычисления текущего статуса
    If pbar IsNot Nothing Then                                      ' Текущий статус операции
      cnt = 0
      For i = 0 To rngs.Count - 1                                   ' По выбранным вкладкам
        For j = 0 To rngs(i).PLC.Count - 1                          ' По сигналам
          For k = 0 To rngs(i).PLC(j).Values.Count - 1              ' По уставкам сигналов в контроллере
            If rngs(i).PLC(j).flagIn(k) Then                        ' Для переменных участвующих в операции 
              cnt += 1
            End If
          Next
        Next
      Next

      pStep = 10000.0 / cnt

      pbar.Value = 0
      pbar.Minimum = 0
      pbar.Maximum = 10000
    End If

    Try
      ' Инициализация
      If isWrite Then                                               ' Операция записи
        opcItm = Array.CreateInstance(GetType(Opc.Da.ItemValue), OPCVarsPerCycle)
        For i = 0 To OPCVarsPerCycle - 1
          opcItm(i) = New Opc.Da.ItemValue
        Next
      Else                                                          ' Операция чтения
        opcItm = Array.CreateInstance(GetType(Opc.Da.Item), OPCVarsPerCycle)
        For i = 0 To OPCVarsPerCycle - 1
          opcItm(i) = New Opc.Da.Item
        Next
      End If

      indxs = Array.CreateInstance(GetType(UInteger), OPCVarsPerCycle)  ' Для поиска данных

      LastRng = 0
      EndRng = rngs.Count - 1

      While True
        If m_IsExit Then Return

        ' Готовим данные
        cnt = 0
        CurRow = LastRow : CurCell = LastCell

        While True
          For i = LastRng To EndRng                                 ' По выбранным вкладкам
            For j = CurRow To rngs(i).PLC.Count - 1                 ' По сигналам
              For k = CurCell To rngs(i).PLC(j).Values.Count - 1    ' По уставкам сигналов в контроллере
                If rngs(i).PLC(j).flagIn(k) Then                    ' К этому моменту данные преобразованы, имена переменных проверены 
                  If cnt >= OPCVarsPerCycle Then Exit While

                  rngs(i).PLC(j).flagOut(k) = False                 ' Сбрасываем результат

                  If isWrite Then opcItm(cnt).Value = rngs(i).PLC(j).Values(k) ' Операция записи
                  opcItm(cnt).ItemName = rngs(i).PLC(j).Names(k)

                  indxs(cnt) = i + j * 100 + k * 100000
                  cnt += 1
                End If
              Next
              CurCell = 0
            Next
            CurRow = 0
          Next

          Exit While
        End While

        LastRng = i : LastRow = j : LastCell = k

        If cnt = 0 Then Exit While

        If cnt < OPCVarsPerCycle Then
          Array.Resize(opcItm, cnt)                                 ' Корректируем размер массива
          Array.Resize(indxs, cnt)                                  ' Корректируем размер массива
        End If

        If m_IsExit Then Return                                     ' Запрос на выход

        ' Записываем / читаем данные в ПЛК
        If isWrite Then                                             ' Операция записи
          opcRes = OPCSrvr.Write(opcItm)                            ' Запись данных в ПЛК
        Else                                                        ' Операция чтения
          opcRes = OPCSrvr.Read(opcItm)                             ' Чтение данных из ПЛК
        End If

        If m_IsExit Then Return                                     ' Запрос на выход

        ' Обрабатываем данные
        For cnt = 0 To indxs.Length - 1
          i = indxs(cnt) Mod 100 : j = (indxs(cnt) / 100) Mod 1000 : k = indxs(cnt) / 100000

          If isWrite Then
            If opcRes(cnt).ResultID <> Opc.ResultID.S_OK Then       ' Ошибка записи
              dbg.AT_DbgMsg("OPC. Ошибка записи " & opcRes(cnt).ItemName & ". " &
                        Replace(OPCSrvr.GetErrorText("en-US", opcRes(cnt).ResultID), vbCrLf, "."), Services.MsgTypes.MsgError)
            Else
              rngs(i).PLC(j).flagOut(k) = True  ' Данные успешно записаны  !!! имя сигнала
              'AT_SCADAMsg(ProgramName & ". В контроллер сохранена уставка '" & SPsIDsRules(rngs(i).ID)(k)(3) &
              '            "' сигнала '" & GetSignalName(rngs(i).ID, rngs(i).Rows(j)) &
              '            "'. Сохраненное значение '" & rngs(i).Rows(j).DBValues(k).ToString & "'")
            End If
          Else
            If opcRes(cnt).ResultID <> Opc.ResultID.S_OK Then  ' Ошибка чтения
              dbg.AT_DbgMsg("OPC. Ошибка чтения " & opcRes(cnt).ItemName & ". " &
                        Replace(OPCSrvr.GetErrorText("en-US", opcRes(cnt).ResultID), vbCrLf, "."), Services.MsgTypes.MsgError)
            ElseIf opcRes(cnt).Quality.GetCode <> 192 Then     ' Данные с плохим качеством
              dbg.AT_DbgMsg("OPC. Ошибка чтения " & opcRes(cnt).ItemName & ". Плохое качество данных. Quality " & opcRes(cnt).Quality.GetCode, Services.MsgTypes.MsgError)
            Else                                            ' Чтение успешно
              rngs(i).PLC(j).Values(k) = opcRes(cnt).Value
              rngs(i).PLC(j).flagOut(k) = True
            End If
          End If
        Next

        ' Текущий статус операции
        If pbar IsNot Nothing Then
          pCur = pCur + pStep * indxs.Length

          If pCur > pbar.Maximum Then
            pbar.Value = pbar.Maximum
          Else
            pbar.Value = pCur
          End If
        End If

        Services.WaitForAWhile(OPCCycleSleepTime,, m_IsExit)                         ' Таймаут между запросами
      End While

      If pbar IsNot Nothing Then
        pCur = pbar.Maximum
        pbar.Value = pbar.Maximum
      End If

    Catch ex As Exception
      dbg.AT_DbgMsg("OPC. Ошибка. " & ex.Message, Services.MsgTypes.MsgError)
    Finally
      ' Освобождение памяти
      indxs = Nothing
      opcItm = Nothing
      opcRes = Nothing

      AT_CloseOPCServer(OPCSrvr)                                    ' Закрываем OPC сервер
    End Try
  End Sub





End Module
