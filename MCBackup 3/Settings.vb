﻿Imports System.Configuration

'Cette classe vous permet de gérer des événements spécifiques dans la classe de paramètres :
' L'événement SettingChanging est déclenché avant la modification d'une valeur de paramètre.
' L'événement PropertyChanged est déclenché après la modification d'une valeur de paramètre.
' L'événement SettingsLoaded est déclenché après le chargement des valeurs de paramètre.
' L'événement SettingsSaving est déclenché avant l'enregistrement des valeurs de paramètre.
Partial Friend NotInheritable Class MySettings
    Public Sub MySettings_SettingsLoaded() Handles MyBase.SettingsLoaded
        Log.Print("Loading settings...", Log.Level.Debug)
        For Each Setting As SettingsPropertyValue In My.Settings.PropertyValues
            Log.Print("{0,-30}= {1}", Log.Level.Debug, Setting.Name, Setting.PropertyValue)
        Next
    End Sub

    Public Sub MySettings_SettingChanging(sender As Object, e As SettingChangingEventArgs) Handles MyBase.SettingChanging
        Log.Print("Setting '{0}' set to '{1}'", Log.Level.Debug, e.SettingName, e.NewValue)
    End Sub

    Public Sub MySettings_SettingsSaving() Handles MyBase.SettingsSaving
        Log.Print("Saving settings...")
        For Each Setting As System.Configuration.SettingsPropertyValue In My.Settings.PropertyValues
            Log.Print("{0,-30}= {1}", Log.Level.Debug, Setting.Name, Setting.PropertyValue)
        Next
    End Sub
End Class
