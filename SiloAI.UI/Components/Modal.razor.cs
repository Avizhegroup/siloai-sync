using Microsoft.AspNetCore.Components.Web;

namespace SiloAI.UI.Components;

public partial class Modal
{
    [Parameter] public ModalButtons Buttons { get; set; } = ModalButtons.OKClose;
    [Parameter] public ModalSize Size { get; set; } = ModalSize.Normal;
    [Parameter] public string ModalTitle { get; set; }
    [Parameter] public string CloseBtnText { get; set; } = "Close";
    [Parameter] public string CloseBtnClass { get; set; } = "btn-secondary";
    [Parameter] public string OkBtnText { get; set; } = "OK";
    [Parameter] public string OkBtnClass { get; set; } = "btn-primary";
    [Parameter] public string CancelBtnText { get; set; } = "Cancel";
    [Parameter] public string CancelBtnClass { get; set; } = "btn-secondary";
    [Parameter] public string YesBtnText { get; set; } = "Yes";
    [Parameter] public string YesBtnClass { get; set; } = "btn-primary";
    [Parameter] public bool DirectionRTL { get; set; }
    [Parameter] public RenderFragment ModalBodyContentHtml { get; set; }
    [Parameter] public RenderFragment ModalFooterHtmlWhitoutCloseBtn { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnCloseModal { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnOpenModal { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnCancelModal { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnYesModal { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnOkModal { get; set; }

    public Guid Guid = Guid.NewGuid();
    public string ModalDisplay = "none;";
    public string ModalClass = "";
    public bool ShowBackdrop = false;

    public async Task Open(MouseEventArgs e)
    {
        ModalDisplay = "block;";
        ModalClass = "Show";
        ShowBackdrop = true;
        StateHasChanged();
        await OnOpenModal.InvokeAsync(e);
    }

    public async Task Close(MouseEventArgs e)
    {
        ModalDisplay = "none";
        ModalClass = "";
        ShowBackdrop = false;
        StateHasChanged();
        await OnCloseModal.InvokeAsync(e);
    }

    public async Task Yes(MouseEventArgs e)
    {
        ModalDisplay = "none";
        ModalClass = "";
        ShowBackdrop = false;
        StateHasChanged();
        await OnYesModal.InvokeAsync(e);
    }

    public async Task Cancel(MouseEventArgs e)
    {
        ModalDisplay = "none";
        ModalClass = "";
        ShowBackdrop = false;
        StateHasChanged();
        await OnCancelModal.InvokeAsync(e);
    }

    public async Task Ok(MouseEventArgs e)
    {
        ModalDisplay = "none";
        ModalClass = "";
        ShowBackdrop = false;
        StateHasChanged();
        await OnOkModal.InvokeAsync(e);
    }
}

public enum ModalSize
{
    Normal,
    Large,
    ExtraLarge
}

public enum ModalButtons
{
    No,
    OKClose,
    YesCancel,
    Close
}
