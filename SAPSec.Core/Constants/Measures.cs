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

    public static class Absence
    {
        public const string Key = "absence";

        public static class Filters
        {
            public static class Type
            {
                public const string Key = $"{Absence.Key}-type";
                public const string Name = "Type of absence";

                public static class Values
                {
                    public const string Overall = "o";
                    public const string Persistent = "p";

                    public static readonly FilterValueDefinition[] AllValues = [
                        new(Overall, "Overall absence"),
                        new(Persistent, "Persistent absence")
                    ];
                }
            }
        }
    }

    public record FilterValueDefinition(string Value, string Name);
}
