using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CbLib
{
    public class DateRange
    {
        #region Properties
        public bool IsValidDateRange { get; set; } = false;
        public DateTime Start { get; set; } = DateTime.MinValue;
        public DateTime End { get; set; } = DateTime.MinValue;
        public TimeSpan RangeLength { get; set; }

        #endregion


        #region Constructors

        public DateRange() { }
        public DateRange(DateTime start, DateTime end) { SetDateRange(start, end); }
        public DateRange(string dateRangeText) { SetDateRange(dateRangeText); }
        public DateRange(List<DateTime> dates) { SetDateRange(dates); }

        #endregion


        #region Public Methods

        override public string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(Start.ToString())
              .Append(" to ")
              .Append(End.ToString());


            return sb.ToString();
        }




        /// <summary>
        /// Finds and returns the overlapping DateRange between the current DateRange and another
        /// </summary>
        /// <param name="range2"></param>
        /// <returns></returns>
        public DateRange FindOverlap(DateRange range2)
        {
            DateRange range1 = this;
            var latestStart = range1.Start.FindLatest(range2.Start);
            var earliestEnd = range1.End.FindEarliest(range1.End);

            DateRange overlapRange = new DateRange(latestStart, earliestEnd);

            return overlapRange;
        }

        public List<DateRange> SplitBy(DateRangeSplitType splitType = DateRangeSplitType.None, double splitAmount = 0)
        {
            var ranges = new List<DateRange>();
            var start = Start;
            var end = DateTime.MinValue;
            var splitAmountInt = Convert.ToInt32(Math.Round(splitAmount));

            if (End == DateTime.MinValue || splitAmount < 1)
            {
                ranges.Add(new DateRange(Start, End));
                return ranges;
            }


            while (end.Date != End.Date)
            {
                switch (splitType)
                {
                    case DateRangeSplitType.Day:
                        end = start.AddDays(splitAmount);
                        break;
                    case DateRangeSplitType.Week:
                        end = start.AddDays(splitAmountInt * 7);
                        break;
                    case DateRangeSplitType.Month:
                        end = start.AddMonths(splitAmountInt);
                        break;
                    case DateRangeSplitType.Year:
                        end = start.AddYears(splitAmountInt);
                        break;
                    case DateRangeSplitType.None:
                        end = End;
                        break;
                }

                if (end >= End)
                {
                    end = End;
                }

                ranges.Add(new DateRange(start, end));

                if (end < End)
                {
                    start = end.AddDays(1);
                }

            }



            return ranges;
        }


        #region SetDateRange overloads
        /// <summary>
        /// Set the Start and End date using a list of dates
        /// </summary>
        /// <param name="dates"></param>
        public void SetDateRange(List<DateTime> dates) { _setDateRange(dates); }


        /// <summary>
        /// Set the Start and End date using two DateTimes
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public void SetDateRange(DateTime start, DateTime end) { _setDateRange(new List<DateTime> { start, end }); }


        /// <summary>
        /// Set the Start and End date using a string with multiple dates in it
        /// </summary>
        /// <param name="dateRangeText"></param>
        public void SetDateRange(string dateRangeText) { _setDateRange(SplitDateRangeText(dateRangeText)); }


        /// <summary>
        /// Actually sets the Start and End date and then calls CalculateTimeDifference
        /// </summary>
        /// <param name="dates"></param>
        private void _setDateRange(List<DateTime> dates)
        {
            // Fill dates list with DefaultDate until it has 2 dates in it
            while (dates.Count < 2)
            {
                dates.Add(DateTime.MinValue);
            }

            // Find the first date in the range and the last date to set Start and End
            Start = dates.Min();
            End = dates.Max();

            CalculateTimeDifference();
            Validate();
        }

        #endregion


        #endregion


        #region Private Creation Methods
        /// <summary>
        /// Searches a string for anything that looks like a date and returns the list
        /// </summary>
        /// <param name="rangeText"></param>
        /// <returns></returns>
        private List<DateTime> SplitDateRangeText(string rangeText)
        {
            List<DateTime> dates = new();

            // Should expand this to use more patterns at some point
            string dateRegexPattern = @"[0-9]{2}[/][0-9]{2}[/][0-9]{4}\s[0-9]{2}[:][0-9]{2}[:][0-9]{2}\s(AM|PM)";
            Regex rgx = new(dateRegexPattern);
            var dateMatches = rgx.Matches(rangeText, 0).ToList();


            if (dateMatches.Count > 0)
            {
                dateMatches.ForEach(
                    match =>
                    dates.Add(match.ToString().TryParseToDate())
                );
            }


            return dates;
        }


        /// <summary>
        /// Calculates the amount of time between Start and End dates
        /// </summary>
        private void CalculateTimeDifference()
        {
            RangeLength = End.Subtract(Start);
        }


        /// <summary>
        /// Checks that this is a valid date range
        /// </summary>
        private void Validate()
        {
            IsValidDateRange = Start != DateTime.MinValue && End != DateTime.MinValue;
        }

        #endregion
    }
}
