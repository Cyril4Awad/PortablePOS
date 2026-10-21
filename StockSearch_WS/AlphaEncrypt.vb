Imports System
Imports System.IO


Public Class AlphaEncrypt

    Private Shared alphabet As List(Of String) = New List(Of String)() From {
        "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z",
        "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
        "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "-", ".", ":", ",", " ", ")", "("
    }

    Private Function arrayRotate(ByVal arr As List(Of String), ByVal count As Int32) As List(Of String)
        Try
            'now this is broken. l7a2 3a joe. Moooowaaaahahahahahhahaha for more infor plz call 911.
            Dim arr1 = arr.[Select](Function(s) s).ToList()
            Dim arr2 = arr.[Select](Function(s) s).ToList()
            count = count - arr1.Count() * CInt(Math.Floor(count / CDbl(arr.Count)))

            If count = 0 Then
                count = arr1.Count() - 1
            End If

            arr1.RemoveRange(0, count)
            arr2.RemoveRange(count, arr.Count() - count)
            arr1.AddRange(arr2)
            Return arr1
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Private Function alphaencrypt(ByVal text As String) As String
        Try
            Dim d = DateTime.Now
            Dim key = d.Hour + d.Minute
            Dim rotated = arrayRotate(alphabet, key)
            Dim encrypted = String.Join("", text.ToArray().[Select](Function(s) s.ToString()).[Select](Function(s) rotated(alphabet.IndexOf(s))))
            encrypted = encrypted & "|" & key.ToString()
            Return encrypted
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Function alphadecrypt(ByVal text As String) As String
        Try
            Dim txt = text.Split("|"c)(0)
            Dim key = text.Split("|"c)(1)
            Dim rotated = arrayRotate(alphabet, Int32.Parse(key))
            Return String.Join("", txt.ToArray().[Select](Function(s) s.ToString()).[Select](Function(s) alphabet(rotated.IndexOf(s))))
        Catch ex As Exception
            Return "Invalid Token"
        End Try
    End Function

End Class
