using Microsoft.AspNetCore.Components;
using Orange.Application.Abstractions;
using Orange.Application.Approvals.GetInbox;
using Orange.Domain.Common;
using Orange.Domain.Leave;
using Orange.Web.Components.Shared;

namespace Orange.Web.Components.Pages;

public partial class Inbox
{
    [Inject] GetInboxHandler InboxHandler { get; set; } = default!;
    [Inject] ICurrentUser CurrentUser { get; set; } = default!;

    bool _loading = true;
    string? _error;
    List<InboxItem> _items = [];
    RequestDrawer? _drawer { get; set; }

    protected override async Task OnInitializedAsync() => await LoadAsync();

    async Task LoadAsync()
    {
        _loading = true;
        _error = null;
        try
        {
            var result = await InboxHandler.HandleAsync(new());
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
