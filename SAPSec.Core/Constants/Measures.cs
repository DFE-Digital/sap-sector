namespace SAPSec.Core.Constants;

public static class Measures
{
    public static class Primary
    {
        public static class Ks2ExpectedRwm
        {
            public const string Key = "expected-rwm";

            public static class Filters
            {
                public static class Subject
                {
                    public const string Key = $"{Ks2ExpectedRwm.Key}-subject";
                    public const string Name = "Subject";

                    public static class Values
                    {
                        public const string ReadingWritingMaths = "rwm";
                        public const string Reading = "r";
                        public const string Writing = "w";
                        public const string Maths = "m";

                        public static readonly FilterValueDefinition[] AllValues = [
                            new(ReadingWritingMaths, "Reading, writing and maths"),
                            new(Reading, "Reading"),
                            new(Writing, "Writing"),
                            new(Maths, "Maths"),
                        ];
                    }
                }
            }
        }

        public const string Ks2GpsExpected = "expected-gps";
        public const string Ks2GpsHigher = "higher-gps";
        public const string Ks2ReadingScore = "reading-score";
    }

    public record FilterValueDefinition(string Value, string Name);
}
