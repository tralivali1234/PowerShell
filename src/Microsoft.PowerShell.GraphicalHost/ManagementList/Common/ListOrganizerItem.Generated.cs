// -----------------------------------------------------------------------
//  <copyright file="ListOrganizerItem.Generated.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//
//  <auto-generated>
//      This code was generated by a tool. DO NOT EDIT
//
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// -----------------------------------------------------------------------

#region StyleCop Suppression - generated code
using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Microsoft.Management.UI.Internal
{

    /// <summary>
    /// This control is the row in the ListOrganizer and offers editing functionality.
    /// </summary>
    /// <remarks>
    ///
    ///
    /// If a custom template is provided for this control, then the template MUST provide the following template parts:
    ///
    /// 	PART_DeleteButton - A required template part which must be of type Button.  Button which keeps track of whether the row should be deleted.
    /// 	PART_EditBox - A required template part which must be of type TextBox.  Displays the text content in an editable manner.
    /// 	PART_LinkButton - A required template part which must be of type Button.  Displays the text content in a read-only manner and allows single click selection.
    /// 	PART_RenameButton - A required template part which must be of type ToggleButton.  Button which allows for editing the name of the item.
    ///
    /// </remarks>
    [TemplatePart(Name="PART_DeleteButton", Type=typeof(Button))]
    [TemplatePart(Name="PART_EditBox", Type=typeof(TextBox))]
    [TemplatePart(Name="PART_LinkButton", Type=typeof(Button))]
    [TemplatePart(Name="PART_RenameButton", Type=typeof(ToggleButton))]
    [Localizability(LocalizationCategory.None)]
    partial class ListOrganizerItem
    {
        //
        // Fields
        //
        private Button deleteButton;
        private TextBox editBox;
        private Button linkButton;
        private ToggleButton renameButton;

        //
        // TextContentPropertyName dependency property
        //
        /// <summary>
        /// Identifies the TextContentPropertyName dependency property.
        /// </summary>
        public static readonly DependencyProperty TextContentPropertyNameProperty = DependencyProperty.Register( "TextContentPropertyName", typeof(string), typeof(ListOrganizerItem), new PropertyMetadata( String.Empty, TextContentPropertyNameProperty_PropertyChanged) );

        /// <summary>
        /// Gets or sets a value which dictates what binding is used to provide content for the items in the list.
        /// </summary>
        [Bindable(true)]
        [Category("Common Properties")]
        [Description("Gets or sets a value which dictates what binding is used to provide content for the items in the list.")]
        [Localizability(LocalizationCategory.None)]
        public string TextContentPropertyName
        {
            get
            {
                return (string) GetValue(TextContentPropertyNameProperty);
            }
            set
            {
                SetValue(TextContentPropertyNameProperty,value);
            }
        }

        static private void TextContentPropertyNameProperty_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ListOrganizerItem obj = (ListOrganizerItem) o;
            obj.OnTextContentPropertyNameChanged( new PropertyChangedEventArgs<string>((string)e.OldValue, (string)e.NewValue) );
        }

        /// <summary>
        /// Occurs when TextContentPropertyName property changes.
        /// </summary>
        public event EventHandler<PropertyChangedEventArgs<string>> TextContentPropertyNameChanged;

        /// <summary>
        /// Called when TextContentPropertyName property changes.
        /// </summary>
        protected virtual void OnTextContentPropertyNameChanged(PropertyChangedEventArgs<string> e)
        {
            OnTextContentPropertyNameChangedImplementation(e);
            RaisePropertyChangedEvent(TextContentPropertyNameChanged, e);
        }

        partial void OnTextContentPropertyNameChangedImplementation(PropertyChangedEventArgs<string> e);

        /// <summary>
        /// Called when a property changes.
        /// </summary>
        private void RaisePropertyChangedEvent<T>(EventHandler<PropertyChangedEventArgs<T>> eh, PropertyChangedEventArgs<T> e)
        {
            if(eh != null)
            {
                eh(this,e);
            }
        }

        //
        // OnApplyTemplate
        //

        /// <summary>
        /// Called when ApplyTemplate is called.
        /// </summary>
        public override void OnApplyTemplate()
        {
            PreOnApplyTemplate();
            base.OnApplyTemplate();
            this.deleteButton = WpfHelp.GetTemplateChild<Button>(this,"PART_DeleteButton");
            this.editBox = WpfHelp.GetTemplateChild<TextBox>(this,"PART_EditBox");
            this.linkButton = WpfHelp.GetTemplateChild<Button>(this,"PART_LinkButton");
            this.renameButton = WpfHelp.GetTemplateChild<ToggleButton>(this,"PART_RenameButton");
            PostOnApplyTemplate();
        }

        partial void PreOnApplyTemplate();

        partial void PostOnApplyTemplate();

        //
        // Static constructor
        //

        /// <summary>
        /// Called when the type is initialized.
        /// </summary>
        static ListOrganizerItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ListOrganizerItem), new FrameworkPropertyMetadata(typeof(ListOrganizerItem)));
            StaticConstructorImplementation();
        }

        static partial void StaticConstructorImplementation();

    }
}
#endregion
