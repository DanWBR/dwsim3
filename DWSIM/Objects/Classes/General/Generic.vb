'    Miscelaneous Classes
'    Copyright 2008 Daniel Wagner O. de Medeiros
'
'    This file is part of DWSIM.
'
'    DWSIM is free software: you can redistribute it and/or modify
'    it under the terms of the GNU General Public License as published by
'    the Free Software Foundation, either version 3 of the License, or
'    (at your option) any later version.
'
'    DWSIM is distributed in the hope that it will be useful,
'    but WITHOUT ANY WARRANTY; without even the implied warranty of
'    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'    GNU General Public License for more details.
'
'    You should have received a copy of the GNU General Public License
'    along with DWSIM.  If not, see <http://www.gnu.org/licenses/>.

Imports Microsoft.MSDN.Samples.GraphicObjects

Namespace DWSIM.Outros

    <System.Serializable()> <System.Runtime.InteropServices.ComVisible(False)> Public Class StatusChangeEventArgs

        Protected m_tag As String = ""
        Protected m_nome As String = ""
        Protected m_tipo As TipoObjeto = TipoObjeto.Nenhum
        Protected m_calculado As Boolean = False
        Protected m_sender As String = ""

        Public Property Emissor() As String
            Get
                Return m_sender
            End Get
            Set(ByVal value As String)
                m_sender = value
            End Set
        End Property

        Public Property Calculado() As Boolean
            Get
                Return m_calculado
            End Get
            Set(ByVal value As Boolean)
                m_calculado = value
            End Set
        End Property

        Public Property Tag() As String
            Get
                Return m_tag
            End Get
            Set(ByVal value As String)
                m_tag = value
            End Set
        End Property

        Public Property Nome() As String
            Get
                Return m_nome
            End Get
            Set(ByVal value As String)
                m_nome = value
            End Set
        End Property

        Public Property Tipo() As TipoObjeto
            Get
                Return m_tipo
            End Get
            Set(ByVal value As TipoObjeto)
                m_tipo = value
            End Set
        End Property

    End Class

    <CLSCompliant(True)> <System.Serializable()> Public Class NodeItem

        Private m_checked As Boolean = False
        Private m_text As String
        Private m_value As String
        Private m_unit As String
        Private m_level As Integer = 0
        Private m_parentnode As String
        Private m_key As Integer

        Sub New()

        End Sub

        Sub New(ByVal texto As String, ByVal valor As String, ByVal unidade As String, ByVal key As Integer, ByVal nivel As Integer, ByVal pai As String)
            Me.m_value = valor
            Me.m_unit = unidade
            Me.m_text = texto
            Me.m_key = key
            Me.m_parentnode = pai
            Me.m_level = nivel
        End Sub

        Public Property Checked() As Boolean
            Get
                Return m_checked
            End Get
            Set(ByVal value As Boolean)
                m_checked = value
            End Set
        End Property

        Public Property Text() As String
            Get
                Return m_text
            End Get
            Set(ByVal value As String)
                m_text = value
            End Set
        End Property

        Public Property Value() As String
            Get
                Return m_value
            End Get
            Set(ByVal value As String)
                m_value = value
            End Set
        End Property

        Public Property Unit() As String
            Get
                Return m_unit
            End Get
            Set(ByVal value As String)
                m_unit = value
            End Set
        End Property

        Public Property Level() As Integer
            Get
                Return m_level
            End Get
            Set(ByVal value As Integer)
                m_level = value
            End Set
        End Property

        Public Property ParentNode() As String
            Get
                Return m_parentnode
            End Get
            Set(ByVal value As String)
                m_parentnode = value
            End Set
        End Property

        Public Property Key() As String
            Get
                Return m_key
            End Get
            Set(ByVal value As String)
                m_key = value
            End Set
        End Property

    End Class

    <System.Serializable()> Public Class Annotation

        Protected m_ann(1) As String
        Protected m_text As String = ""
        Protected m_rtfText As String = ""

        Sub New(ByVal rtf As String, ByVal text As String)
            m_rtfText = rtf
            m_text = text
        End Sub

        Sub New()

        End Sub

        Public Property annotation() As Object
            Get
                Return m_rtfText
            End Get
            Set(ByVal value As Object)
                m_rtfText = value(0)
                m_text = value(1)
            End Set
        End Property

        Public Overrides Function ToString() As String
            Return m_text
        End Function

    End Class

    <System.Serializable()> Public Class WatchItem

        Public ObjID As String = ""
        Public PropID As String = ""
        Public ROnly As Boolean = False

        Sub New()

        End Sub

        Sub New(ByVal oid As String, ByVal pid As String, ByVal ro As Boolean)
            ObjID = oid
            PropID = pid
            ROnly = ro
        End Sub

    End Class

End Namespace
