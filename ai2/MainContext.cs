using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Misc;

class MainContext
{
    public class Configuration
    {
        public Misc.Vector2<int> imageSize;
        public int radialKernelSize;

        public int radialKernelPositionsLength;

        public int attentionDownsamplePower;
        public float attentionForgettingFactor;
    }

    public void configure(Configuration configuration)
    {
        this.configuration = configuration;
    }

    public void initialize()
    {
        computeContext = new ComputationBackend.OpenCl.ComputeContext();
        computeContext.initialize();

        initializeOperatorRadialKernel();
        initializeOperatorBlur();
        initializeOperatorFindNearestPosition();
        initializeAttentionModule();
        initializeOperatorSkeletalize();
    }

    private void initializeOperatorSkeletalize()
    {
        operatorSkeletalize.initialize(computeContext, configuration.imageSize);
    }

    private void initializeAttentionModule()
    {
        attentionModule.featureMapDownsamplePower = configuration.attentionDownsamplePower;
        attentionModule.forgettingFactor = configuration.attentionForgettingFactor;

        attentionModule.initialize(computeContext, configuration.imageSize);
    }

    private void initializeOperatorRadialKernel()
    {
        int kernelPositionsLength;
        
        operatorRadialKernel = new ComputationBackend.OpenCl.OperatorRadialKernel();

        operatorRadialKernel.createKernel(configuration.radialKernelSize);


        kernelPositionsLength = ((configuration.imageSize.x / 4) - 1) * (configuration.imageSize.y / 4);

        operatorRadialKernel.initialize(computeContext, configuration.radialKernelPositionsLength, configuration.imageSize);
    }

    private void initializeOperatorBlur()
    {
        operatorBlur = new ComputationBackend.OpenCl.OperatorBlur();
        operatorBlur.initialize(computeContext, 3, configuration.imageSize);

    }

    private void initializeOperatorFindNearestPosition()
    {
        operatorFindNearest = new ComputationBackend.OpenCl.OperatorFindNearestPosition();
        operatorFindNearest.initialize(computeContext, 20, configuration.imageSize);
    }

    /**
     * calculate the result of the radial kernel (rbf) on the OpenCL device
     * 
     * 
     * 
     */
    public void calculateRadialKernel(ResourceMetric resourceMetric, Map2d<float> inputMap, Vector2<int>[] radialKernelPositions, ref float[] radialKernelResults)
    {
        operatorRadialKernel.inputMap = inputMap;
        operatorRadialKernel.kernelPositions = radialKernelPositions;

        resourceMetric.startTimer("visual", "rbf calculation", "");
        operatorRadialKernel.calculate(computeContext);
        
        // pull out the result
        operatorRadialKernel.kernelResults.CopyTo(radialKernelResults, 0);

        resourceMetric.stopTimer();
    }

    public void calculateOperatorBlur(ResourceMetric resourceMetric, Map2d<float> inputMap, Map2d<float> outputMap)
    {
        operatorBlur.inputMap = inputMap;
        operatorBlur.outputMap = outputMap;

        resourceMetric.startTimer("visual", "blur", "");
        operatorBlur.calculate(computeContext);
        resourceMetric.stopTimer();
    }

    public void calculateOperatorFindNearest(ResourceMetric resourceMetric, Map2d<bool> inputMap, Vector2<int>[] inputPositions, ref bool[] foundNewPositions, ref Vector2<int>[] outputPositions)
    {
        operatorFindNearest.inputMap = inputMap;
        operatorFindNearest.inputPositions = inputPositions;

        resourceMetric.startTimer("visual", "retrack border pixels", "");
        operatorFindNearest.calculate(computeContext);
        resourceMetric.stopTimer();

        foundNewPositions = operatorFindNearest.foundNewPositions;
        outputPositions = operatorFindNearest.outputPositions;
    }

    public void calculateAttentionModule(ResourceMetric resourceMetric, Map2d<float> motionMap)
    {
        attentionModule.motionMap = motionMap;

        attentionModule.calculate(resourceMetric, computeContext);
    }

    public void calculateOperatorSkeletalize(ResourceMetric resourceMetric, Map2d<bool> inputMap, Map2d<bool> resultMap)
    {
        operatorSkeletalize.inputMap = inputMap;
        operatorSkeletalize.resultMap = resultMap;

        operatorSkeletalize.calculate(computeContext, resourceMetric);
    }

    public Map2d<float> attentionModuleGetMasterMap()
    {
        return attentionModule.getMasterNovelityAsMap();
    }

    public ComputationBackend.cs.AttentionModule getAttentionModule()
    {
        return attentionModule;
    }


    private Configuration configuration;

    private ComputationBackend.OpenCl.OperatorRadialKernel operatorRadialKernel;
    private ComputationBackend.OpenCl.OperatorBlur operatorBlur;
    private ComputationBackend.OpenCl.OperatorFindNearestPosition operatorFindNearest;
    private ComputationBackend.cs.AttentionModule attentionModule = new ComputationBackend.cs.AttentionModule();
    private ComputationBackend.OpenCl.OperatorSkeletalize operatorSkeletalize = new ComputationBackend.OpenCl.OperatorSkeletalize();

    // NOTE< for development public >
    public ComputationBackend.OpenCl.ComputeContext computeContext;
}

