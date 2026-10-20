using Microsoft.AspNetCore.Components;
using Orange.Domain.Common;

namespace Orange.Web.Components.Shared;

public partial class RequestDrawer
{
    bool _open;
    LeaveRequestId? _requestId;
    Func<Task>? _onActed;

    public void Show(LeaveRequestId id, Func<Task>? onActed = null)
    {
        _requestId = id;
        _onActed = onActed;
        _open = true;
        StateHasChanged();
    }
}
