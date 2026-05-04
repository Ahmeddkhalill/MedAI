namespace MedAI.Errors;

public record XrayErrors
{
    public static readonly Error Unauthorized
        = new("Xray.Unauthorized", "User not authorized", StatusCodes.Status401Unauthorized);

    public static readonly Error PendingReview = 
        new("Xray.PendingReview", "You already have an X-ray awaiting review. Please wait until it is revised.", StatusCodes.Status400BadRequest);

    public static readonly Error NotFound =
        new("Xray.NotFound", "X-ray not found", StatusCodes.Status404NotFound);

    public static readonly Error AlreadyRevised =
        new("Xray.AlreadyRevised", "X-ray has already been revised", StatusCodes.Status400BadRequest);

    public static readonly Error NotRevisedYet = 
        new("Xray.NotRevisedYet", "X-ray has not been revised yet", StatusCodes.Status400BadRequest);

    public static readonly Error InvalidAIResponse =
        new("Xray.InvalidAiResponse", "Received an invalid response from the AI service. Please try again later.", StatusCodes.Status502BadGateway);

    public static readonly Error AIServiceUnavailable =
        new("Xray.AiServiceUnavailable", "AI service is currently unavailable. Please try again later.", StatusCodes.Status503ServiceUnavailable);
}
