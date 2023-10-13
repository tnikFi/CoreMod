using Discord;
using Discord.Interactions;

namespace Web.Discord.Components.Modals;

public class ReportMessageModal : IModal
{
    public const string Id = "report_message";

    [InputLabel("Reason")]
    [ModalTextInput("report_reason", TextInputStyle.Paragraph,
        "Describe how this message violates the rules of the server.", maxLength: 1000)]
    public string? Reason { get; set; }

    public string Title => "Report Message";
}