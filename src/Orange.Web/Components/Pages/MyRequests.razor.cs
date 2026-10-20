using Microsoft.AspNetCore.Components;
using Orange.Application.Abstractions;
using Orange.Application.Requests.GetMyRequests;
using Orange.Domain.Common;
using Orange.Domain.Leave;
using Orange.Web.Components.Shared;

namespace Orange.Web.Components.Pages;

public partial class MyRequests
{
    [Inject] GetMyRequestsHandler RequestsHandler { get; set; } = default!;
    [Inject] ICurrentUser CurrentUser { get; set; } = default!;

    [Parameter] public string? RequestIdParam { get; set; }

    bool _loading = true;
    string? _error;
    RequestStatus? _statusFilter;
    int? _yearFilter = DateTime.Today.Year;
    List<LeaveRequestSummary> _items = [];
    RequestDrawer? _drawer { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
        if (!string.IsNullOrEmpty(RequestIdParam) && Guid.TryParse(RequestIdParam, out var g))
            OpenDrawer(new(g));
    }

    async Task LoadAsync()
    {
        _loading = true;
        _error = null;
        try
        {
            var result = await RequestsHandler.HandleAsync(new(
                Page: 1, PageSize: 100,
                Status: _statusFilter,
                Year: _yearFilter));
            _items = result.Items.ToList();
        }
        catch (Exception ex)
        {
            _error = ex.Message;
        }
        finally { _loading = false; }
    }

    void OpenDrawer(LeaveRequestId id) => _drawer?.Show(id, LoadAsync);

    static string LeaveTypeLabel(LeaveType t) => t switch
    {
        LeaveType.Annual => "Annual leave",
        LeaveType.Sick   => "Sick leave",
        LeaveType.Unpaid => "Unpaid leave",
        _                => t.ToString(),
    };
}
