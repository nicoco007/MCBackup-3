'   ╔═══════════════════════════════════════════════════════════════════════════╗
'   ║                      Copyright © 2013-2015 nicoco007                      ║
'   ║                                                                           ║
'   ║      Licensed under the Apache License, Version 2.0 (the "License");      ║
'   ║      you may not use this file except in compliance with the License.     ║
'   ║                  You may obtain a copy of the License at                  ║
'   ║                                                                           ║
'   ║                 http://www.apache.org/licenses/LICENSE-2.0                ║
'   ║                                                                           ║
'   ║    Unless required by applicable law or agreed to in writing, software    ║
'   ║     distributed under the License is distributed on an "AS IS" BASIS,     ║
'   ║  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. ║
'   ║     See the License for the specific language governing permissions and   ║
'   ║                      limitations under the License.                       ║
'   ╚═══════════════════════════════════════════════════════════════════════════╝

Imports System.Configuration

'Cette classe vous permet de gérer des événements spécifiques dans la classe de paramètres :
' L'événement SettingChanging est déclenché avant la modification d'une valeur de paramètre.
' L'événement PropertyChanged est déclenché après la modification d'une valeur de paramètre.
' L'événement SettingsLoaded est déclenché après le chargement des valeurs de paramètre.
' L'événement SettingsSaving est déclenché avant l'enregistrement des valeurs de paramètre.
Partial Friend NotInheritable Class MySettings
    Public Sub MySettings_SettingsLoaded() Handles MyBase.SettingsLoaded
        Log.Debug("Loading settings...")
        For Each Setting As SettingsPropertyValue In My.Settings.PropertyValues
            Log.Verbose("{0,-45}= {1}", Setting.Name, Setting.PropertyValue)
        Next
    End Sub

    Public Sub MySettings_SettingChanging(sender As Object, e As SettingChangingEventArgs) Handles MyBase.SettingChanging
        Log.Debug("Setting '{0}' set to '{1}'", e.SettingName, e.NewValue)
    End Sub

    Public Sub MySettings_SettingsSaving() Handles MyBase.SettingsSaving
        Log.Debug("Saving settings...")
        For Each Setting As SettingsPropertyValue In My.Settings.PropertyValues
            Log.Verbose("{0,-30}= {1}", Setting.Name, Setting.PropertyValue)
        Next
    End Sub
End Class
