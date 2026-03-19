using System.Net.Http.Json;
using System.Text.Json;
using EquipmentDashboard.Models;

namespace EquipmentDashboard;

public partial class MainForm : Form
{
    private static readonly HttpClient _httpClient = new HttpClient
    {
        BaseAddress = new Uri("https://localhost:7226") // CHANGE THIS
    };

    private List<EquipmentResponseDto> _equipment = new();

    public MainForm()
    {
        InitializeComponent();
        _ = LoadEquipmentAsync();
    }

    private async Task LoadEquipmentAsync()
    {
        try
        {
            SetStatus("Loading...", Color.Gray);

            var response = await _httpClient.GetAsync("/equipments");

            if (!response.IsSuccessStatusCode)
            {
                SetStatus($"Error: {response.StatusCode}", Color.Red);
                return;
            }

            string json = await response.Content.ReadAsStringAsync();

            _equipment = JsonSerializer.Deserialize<List<EquipmentResponseDto>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<EquipmentResponseDto>();

            lstEquipment.Items.Clear();
            foreach (var item in _equipment)
                lstEquipment.Items.Add(item.Name);

            SetStatus(_equipment.Count > 0 ? "" : "No equipment found.", Color.Gray);
        }
        catch (HttpRequestException)
        {
            SetStatus("Cannot connect to EquipmentAPI. Is it running?", Color.Red);
        }
    }

    private void lstEquipment_SelectedIndexChanged(object sender, EventArgs e)
    {
        int index = lstEquipment.SelectedIndex;

        if (index < 0 || index >= _equipment.Count)
        {
            lblNoSelection.Visible = true;
            pnlDetailFields.Visible = false;
            return;
        }

        var item = _equipment[index];

        lblIdValue.Text = item.Id.ToString();
        lblNameValue.Text = item.Name;
        lblCategoryValue.Text = item.Category;
        lblStatusValue.Text = item.Status;
        lblLocationValue.Text = item.Location;

        lblNoSelection.Visible = false;
        pnlDetailFields.Visible = true;
    }

    private async void btnCreate_Click(object sender, EventArgs e)
    {
        string name = txtName.Text.Trim();
        string category = txtCategory.Text.Trim();
        string status = txtStatus.Text.Trim();
        string location = txtLocation.Text.Trim();

        if (string.IsNullOrEmpty(name))
        {
            SetStatus("Name is required.", Color.OrangeRed);
            txtName.Focus();
            return;
        }

        if (string.IsNullOrEmpty(category))
        {
            SetStatus("Category is required.", Color.OrangeRed);
            txtCategory.Focus();
            return;
        }

        if (string.IsNullOrEmpty(status))
        {
            SetStatus("Status is required.", Color.OrangeRed);
            txtStatus.Focus();
            return;
        }

        if (string.IsNullOrEmpty(location))
        {
            SetStatus("Location is required.", Color.OrangeRed);
            txtLocation.Focus();
            return;
        }

        var dto = new CreateEquipmentDto
        {
            Name = name,
            Category = category,
            Status = status,
            Location = location
        };

        try
        {
            btnCreate.Enabled = false;
            SetStatus("Creating...", Color.Gray);

            var response = await _httpClient.PostAsJsonAsync("/equipments", dto);

            if (response.IsSuccessStatusCode)
            {
                txtName.Clear();
                txtCategory.Clear();
                txtStatus.Clear();
                txtLocation.Clear();

                SetStatus($"Equipment '{name}' created successfully.", Color.SeaGreen);

                await LoadEquipmentAsync();
            }
            else
            {
                SetStatus($"Failed: {response.StatusCode}", Color.Red);
            }
        }
        catch (HttpRequestException)
        {
            SetStatus("Cannot connect to EquipmentAPI.", Color.Red);
        }
        finally
        {
            btnCreate.Enabled = true;
        }
    }

    private void SetStatus(string message, Color color)
    {
        lblStatusMsg.Text = message;
        lblStatusMsg.ForeColor = color;
    }
}
