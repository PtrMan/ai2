using System;
using System.Collections.Generic;

using System.Diagnostics;
using System.Threading;

class ResourceMetric
{
    private class TimingInformation
    {
        public string calculationGroup;
        public string specififcGroup;
        public string group;
        public long timingInNanoseconds;
    }

    public void startTimer(string group, string calculationGroup, string specificGroup)
    {
        lastCalculationGroup = calculationGroup;
        lastSpecificGroup = specificGroup;
        lastGroup = group;
        stopwatch.Restart();
    }

    public void stopTimer()
    {
        TimingInformation newTimingInformation;

        stopwatch.Stop();

        newTimingInformation = new TimingInformation();
        newTimingInformation.calculationGroup = lastCalculationGroup;
        newTimingInformation.specififcGroup = lastSpecificGroup;
        newTimingInformation.group = lastGroup;
        newTimingInformation.timingInNanoseconds = stopwatch.ElapsedTicks * 100;

        timingInformations.Add(newTimingInformation);
    }

    public void reset()
    {
        timingInformations.Clear();
    }

    // prints the timing report
    public void report()
    {
        long sumOfTime;

        sumOfTime = 0;

        foreach( TimingInformation iterationTimingInformation in timingInformations )
        {
            string reportString;

            reportString = string.Format("={0}.{1}:{2}  took {3} us", iterationTimingInformation.group, iterationTimingInformation.calculationGroup, iterationTimingInformation.specififcGroup, iterationTimingInformation.timingInNanoseconds / 1000);
            sumOfTime += iterationTimingInformation.timingInNanoseconds;

            Console.WriteLine(reportString);
        }

        Console.WriteLine(string.Format("overall time is  {0} us", sumOfTime/1000));
    }

    // TODO< how to get the processThread from a process or a task? >
    static private long getAbsoluteProcessTimeInMs(ProcessThread processThread)
    {
        TimeSpan totalProcessorTimeTimespan;

        totalProcessorTimeTimespan = processThread.TotalProcessorTime;

        return totalProcessorTimeTimespan.Milliseconds;
    }

    private Stopwatch stopwatch = new Stopwatch();
    private string lastCalculationGroup;
    private string lastSpecificGroup;
    private string lastGroup;

    // is stored in a list because on measuring time it is easy addable
    // later the timing informations can be collected and presented to the other systems
    private List<TimingInformation> timingInformations = new List<TimingInformation>();
}

