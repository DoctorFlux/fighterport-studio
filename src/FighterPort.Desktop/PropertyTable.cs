using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace FighterPort.Desktop;

internal sealed class PropertyField(string category,string name,Func<string> read,string description,Action<string>? commit=null):INotifyPropertyChanged
{
    public string Category=>category;
    public string Name=>name;
    public string Description=>description;
    public bool ReadOnly=>commit==null;
    public string Value { get=>read();set { } } // UI input is committed explicitly through the validated transaction below.
    public event PropertyChangedEventHandler? PropertyChanged;
    public bool TryCommit(string value,out string error)
    {
        try{if(commit==null)throw new InvalidOperationException("Read-only source field");commit(value);error="";PropertyChanged?.Invoke(this,new(nameof(Value)));return true;}
        catch(Exception e)when(e is InvalidOperationException or ArgumentException or System.IO.IOException or System.IO.InvalidDataException or FormatException or OverflowException){error=e.Message;PropertyChanged?.Invoke(this,new(nameof(Value)));return false;}
    }
}

internal sealed class PropertyTable:DockPanel
{
    internal readonly DataGrid Grid=new();
    internal readonly TextBlock Description=new(){TextWrapping=TextWrapping.Wrap,Margin=new(4),MinHeight=30};
    internal IReadOnlyList<PropertyField> Fields { get; private set; }=[];
    internal PropertyTable()
    {
        DockPanel.SetDock(Description,Dock.Bottom);Children.Add(Description);Children.Add(Grid);
        Grid.Columns.Add(new DataGridTextColumn{Header="Property",Binding=new Binding(nameof(PropertyField.Name)),IsReadOnly=true,Width=new(145)});
        Grid.Columns.Add(new DataGridTextColumn{Header="Value",Binding=new Binding(nameof(PropertyField.Value)){Mode=BindingMode.TwoWay,UpdateSourceTrigger=UpdateSourceTrigger.Explicit},Width=new(1,DataGridLengthUnitType.Star)});
        Grid.GroupStyle.Add(new GroupStyle{HeaderTemplate=CategoryTemplate()});
        Grid.BeginningEdit+=(_,e)=>{if(((PropertyField)e.Row.Item).ReadOnly)e.Cancel=true;};
        Grid.SelectionChanged+=(_,_)=>{if(Grid.SelectedItem is PropertyField f)Description.Text=f.Description+(f.ReadOnly?" (read-only)":"");};
        Grid.CellEditEnding+=(_,e)=>
        {
            if(e.EditAction!=DataGridEditAction.Commit||e.EditingElement is not TextBox text)return;
            var field=(PropertyField)e.Row.Item;
            if(!field.TryCommit(text.Text,out string error)){e.Cancel=true;Description.Text=error;text.SelectAll();}
            else {text.Text=field.Value;Description.Text=field.Description;}
        };
    }
    private static DataTemplate CategoryTemplate()
    {
        var template=new DataTemplate();var text=new FrameworkElementFactory(typeof(TextBlock));text.SetBinding(TextBlock.TextProperty,new Binding("Name"));text.SetValue(TextBlock.FontWeightProperty,FontWeights.SemiBold);text.SetValue(TextBlock.MarginProperty,new Thickness(4,3,0,2));template.VisualTree=text;return template;
    }
    internal void Show(IEnumerable<PropertyField> fields)
    {
        Grid.CancelEdit();Fields=fields.ToArray();var view=new ListCollectionView(Fields.ToList());view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(PropertyField.Category)));Grid.ItemsSource=view;Description.Text="Select a field for its description. Enter commits; Esc cancels text entry.";
    }
}
