using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ADTool.Contracts.Services;
using ADTool.Models;
using ADTool.Services;
using ADTool.ViewModels;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.IdentityModel.Tokens;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace ADTool.Views;

public partial class Add_Remove_User_GroupPage : Page
{
    private readonly IADServices _ADServices;
    private readonly ILogService _LogService;

    private string[] ClearItems = {""};
    private Dictionary<string, string> UserNames;
    private Dictionary<string, string> GroupNames;

    public Add_Remove_User_GroupPage(Add_Remove_User_GroupViewModel viewModel, IADServices aDServices, ILogService logService)
    {
        InitializeComponent();
        DataContext = viewModel;
        _ADServices = aDServices;
        _LogService = logService;
    }

    private void Selection_Changed(object sender, SelectionChangedEventArgs e)
    {
        try
        {

            if (Username_Combobox.SelectedItem != null)
            {
                string selectedKey = Username_Combobox.SelectedItem.ToString();
                string selectedValue = UserNames[selectedKey]; // Assuming GroupNames is your dictionary
                Group_username_Textbox.Visibility = Visibility.Visible;
                Group_username_Textbox.IsEnabled = true;
                Username_Combobox.Visibility = Visibility.Hidden;
                Username_Combobox.IsEnabled = false;


                Group_username_Textbox.Text = selectedKey;
                Username_Combobox.ItemsSource = ClearItems;
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"An error occurred: {ex.Message}", "Attention", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Check_Names(object sender, EventArgs e)
    {
        if (!Group_username_Textbox.Text.IsNullOrEmpty())
        {
            string searchfilter_groupName = Group_username_Textbox.Text;
            UserNames = _ADServices.FindUsers(searchfilter_groupName);
            if (UserNames.Keys.Count > 1)
            {
                Group_username_Textbox.Visibility = Visibility.Hidden;
                Group_username_Textbox.IsEnabled = false;
                Username_Combobox.Visibility = Visibility.Visible;
                Username_Combobox.IsEnabled = true;
                Username_Combobox.ItemsSource = UserNames.Keys;
            }
            else if(UserNames.Keys.Count == 1)
            {
                Group_username_Textbox.Text = UserNames.Keys.FirstOrDefault();
            }
            
        }
    }

    private void Selection_Changed_GroupName(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (GroupName_Combobox.SelectedItem != null)
            {
                string selectedKey = GroupName_Combobox.SelectedItem.ToString();
                string selectedValue = GroupNames[selectedKey]; // Assuming GroupNames is your dictionary
                Group_name_Textbox.Visibility = Visibility.Visible;
                Group_name_Textbox.IsEnabled = true;
                GroupName_Combobox.Visibility = Visibility.Hidden;
                GroupName_Combobox.IsEnabled = false;


                Group_name_Textbox.Text = selectedKey;
                
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"An error occurred: {ex.Message}", "Attention", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Check_Names_Group(object sender, EventArgs e)
    {
        if (!Group_name_Textbox.Text.IsNullOrEmpty())
        {
            string searchfilter_groupName = Group_name_Textbox.Text;
            GroupNames = _ADServices.FindGroups(searchfilter_groupName);
            if (GroupNames.Keys.Count > 1)
            {
                Group_name_Textbox.Visibility = Visibility.Hidden;
                Group_name_Textbox.IsEnabled = false;
                GroupName_Combobox.Visibility = Visibility.Visible;
                GroupName_Combobox.IsEnabled = true;
                GroupName_Combobox.ItemsSource = GroupNames.Keys;
            }
            else if (GroupNames.Keys.Count == 1)
            {
                Group_name_Textbox.Text = GroupNames.Keys.FirstOrDefault();
            }
        }
    }


    private void AddUserToGroup(object sender, EventArgs e)
    {
        try
        {
            string groupnameTest = Group_name_Textbox.Text;
            bool checkGroup = _ADServices.IsGroupInLocalAD(groupnameTest);
            if (checkGroup)
            {
                string groupname = GroupNames[Group_name_Textbox.Text];
                bool test = _ADServices.AddUserToGroup(Group_username_Textbox.Text, groupname);
                if (test)
                {
                    _LogService.createLog("addToGroup", $"username:{Group_username_Textbox.Text}/group:{groupname}");
                    MessageBox.Show($"The group {groupnameTest} has been added to the user {Group_username_Textbox.Text}", "Attention", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                UserNameGrid.Visibility = Visibility.Visible;
                GroupnameGrid.Visibility = Visibility.Hidden;
                SubmitButton.Visibility = Visibility.Visible;
                AddButton.Visibility = Visibility.Hidden;
                Label.Content = "Please enter the Username:";
                Button button = SubmitButton;
                button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            }
            else
            {
                MessageBox.Show($"The group {groupnameTest} is not located in AD", "Attention", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred: {ex.Message}", "Attention", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Add_Group(object sender, MouseButtonEventArgs e)
    {
        MessageBoxResult resutl = System.Windows.MessageBox.Show($"Do you want to add an groupmembership to {Group_username_Textbox.Text}", "Attention", MessageBoxButton.YesNo, MessageBoxImage.Information);
        if (resutl == MessageBoxResult.Yes)
        {
            Label.Content = "Please enter the Groupname:";
            GroupnameGrid.Visibility = Visibility.Visible;
            UserNameGrid.Visibility = Visibility.Hidden;
            AddButton.Visibility = Visibility.Visible;
            SubmitButton.Visibility = Visibility.Hidden;
        }
    }
}
