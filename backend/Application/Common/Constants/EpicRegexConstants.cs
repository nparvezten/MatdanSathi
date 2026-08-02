namespace MatdarSathi.API.Application.Common.Constants;

public static class EpicRegexConstants
{
    public const string EpicPattern = @"^[A-Z0-9/\-]+$";

    // Extractor regex for EPIC numbers (e.g. ABC1234567, XYZ9876543, MH/01/123) requiring both letters and numbers within the token
    public const string EpicExtractorPattern = @"\b(?=[A-Z0-9/\-]*[A-Za-z])(?=[A-Z0-9/\-]*[0-9])[A-Za-z0-9/\-]{5,20}\b";
}
