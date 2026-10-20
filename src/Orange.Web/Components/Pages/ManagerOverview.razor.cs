using Microsoft.AspNetCore.Components;
using Orange.Application.Abstractions;
using Orange.Application.Approvals.GetFinalApprovals;
using Orange.Application.Approvals.GetInbox;
using Orange.Domain.Common;
using Orange.Domain.Leave;
using Orange.Web.Components.Shared;

namespace Orange.Web.Components.Pages;

public partial class ManagerOverview
{
    [Inject] GetFinalApprovalsHandler FinalHandler { get; set; } = default!;
    [Inject] GetInboxHandler InboxHandler { get; set; } = default!;
    [Inject] ICurrentUser CurrentUser { get; set; } = default!;

    bool _loading = true;
    List<FinalApprovalItem> _finalItems = [];
    int _finalCount;
    int _inboxCount;
    RequestDrawer? _drawer { get; set; }

    protected override async Task OnInitializedAsync() => await ReloadAsync();

    async Task ReloadAsync()
    {
        _loading = true;
        try
        {
            var finalTask = FinalHandler.HandleAsync(new());
            var inboxTask = InboxHandler.HandleAsync(new());
            await Task.WhenAll(finalTask, inboxTask);
            var final = await finalTask;
            var inbox = await inboxTask;
            _finalItems = final.Items.ToList();
            _finalCount = final.TotalItems;
            _inboxCount = inbox.TotalItems;
        }
        finally { _loading = false; }
    }

    void OpenDrawer(LeaveRequestId id) => _drawer?.Show(id, ReloadAsync);

    static string LeaveTypeLabel(LeaveType t) => t switch
    {
        LeaveType.Annual => "Annual leave",
        LeaveType.Sick   => "Sick leave",
        LeaveType.Unpaid => "Unpaid leave",
        _                => t.ToString(),
    };
}
