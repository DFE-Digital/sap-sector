namespace SAPSec.Core.Constants;

public static class Measures
{
    public static class Primary
    {
        public static class Ks2ExpectedRwm
        {
            public const string Key = "expected-rwm";
            public const string Name = "Meeting expected standard in reading, writing and maths";

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

        public static class Ks2HigherRwm
        {
            public const string Key = "higher-rwm";
            public const string Name = "Achieved a higher standard in reading, writing and maths";

            public static class Filters
            {
                public static class Subject
                {
                    public const string Key = $"{Ks2HigherRwm.Key}-subject";
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

        public static class Ks2ReadingScore
        {
            public const string Key = "reading-score";
            public const string Name = "Average scaled score in reading";
        }

        public static class Ks2MathsScore
        {
            public const string Key = "maths-score";
            public const string Name = "Average scaled score in maths";
        }

        public static class Ks2ExpectedGps
        {
            public const string Key = "expected-gps";
            public const string Name = "Meeting expected standard in grammar, punctuation and spelling";
        }

        public static class Ks2HigherGps
        {
            public const string Key = "higher-gps";
            public const string Name = "Achieved a higher standard in grammar, punctuation and spelling";
        }

    }

    public static class Secondary
    {
        public static class Ks4Attainment8
        {
            public const string Key = "attainment8";
            public const string Name = "Attainment 8";
        }

        public static class Ks4EnglishMaths
        {
            public const string Key = "eng-maths";
            public const string Name = "Grade achieved in English and maths GCSEs";

            public static class Filters
            {
                public static class Grade
                {
                    public const string Key = $"{Ks4EnglishMaths.Key}-grade";
                    public const string Name = "Grade";

                    public static class Values
                    {
                        public const string Grade4AndAbove = "4";
                        public const string Grade5AndAbove = "5";

                        public static readonly FilterValueDefinition[] AllValues = [
                            new(Grade4AndAbove, "Grade 4 and above"),
                            new(Grade5AndAbove, "Grade 5 and above")
                        ];
                    }
                }
            }
        }

        public static class Ks4Destinations
        {
            public const string Key = "destinations";
            public const string Name = "Staying in education or entering employment";

            public static class Filters
            {
                public static class Destination
                {
                    public const string Key = $"{Ks4Destinations.Key}-dest";
                    public const string Name = "Destination";

                    public static class Values
                    {
                        public const string AllDestinations = "all";
                        public const string Education = "ed";
                        public const string Employment = "emp";

                        public static readonly FilterValueDefinition[] AllValues = [
                            new(AllDestinations, "All destinations"),
                            new(Education, "Education"),
                            new(Employment, "Employment and apprenticeships")
                        ];
                    }
                }
            }
        }
    }

    public record FilterValueDefinition(string Value, string Name);
}
