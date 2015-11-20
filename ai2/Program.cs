using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Algorithms;
using Misc;

using System.Diagnostics;

using System.Drawing;

using System.Runtime.InteropServices;
using System.Reflection;

// for reading causal set
using System.IO;

namespace ai2
{
    // is for testing purposes here
    class LinearBorderEntry
    {
        public float[] radialKernelResults;

        public Vector2<int>[] radialKernelPositions;

        public bool[] borderCrossed;

        public float angle; // in radiants

        public int indexOfOtherSide; // in which index does it wrap around the y border?
    }

    class TestDispatcher : ProgramRepresentation.Execution.FunctionalInterpreter.IInvokeDispatcher
    {
        public void dispatchInvokeStart(List<string> path, List<Datastructures.Variadic> parameters, List<ProgramRepresentation.Execution.FunctionalInterpreter.InterpretationState.ScopeLevel> calleeScopeLevels, out Datastructures.Variadic result, out ProgramRepresentation.Execution.FunctionalInterpreter.EnumInvokeDispatcherCallResult callResult)
        {
            if( path.Count == 2 )
            {
                if( path[0] == "array" && path[1] == "at" )
                {
                    Datastructures.Variadic indexVariadic;
                    Datastructures.Variadic arrayVariadic;
                    int index;
                    List<Datastructures.Variadic> array;

                    if( parameters.Count != 2 )
                    {
                        throw new Exception("array.at required two parameters!");
                    }
                
                    indexVariadic = parameters[1];
                    arrayVariadic = parameters[0];

                    if( indexVariadic.type != Datastructures.Variadic.EnumType.INT )
                    {
                        throw new Exception("array.at index is no int");
                    }

                    if( arrayVariadic.type != Datastructures.Variadic.EnumType.ARRAY )
                    {
                        throw new Exception("array.at array is not an array!");
                    }

                    index = indexVariadic.valueInt;
                    array = arrayVariadic.valueArray;

                    result = builtinArrayAt(array, index);

                    callResult = ProgramRepresentation.Execution.FunctionalInterpreter.EnumInvokeDispatcherCallResult.DONE;
                    return;
                }
                else if( path[0] == "math" && path[1] == "sin" )
                {
                    Datastructures.Variadic parameterVariadic;
                    float resultFloat;

                    if (parameters.Count != 1)
                    {
                        throw new Exception("math sin required one parameters!");
                    }

                    parameterVariadic = parameters[0];

                    if( parameterVariadic.type != Datastructures.Variadic.EnumType.FLOAT )
                    {
                        throw new Exception("math sin parameter must be a float");
                    }

                    resultFloat = (float)System.Math.Sin(parameterVariadic.valueFloat);

                    result = new Datastructures.Variadic(Datastructures.Variadic.EnumType.FLOAT);
                    result.valueFloat = resultFloat;

                    callResult = ProgramRepresentation.Execution.FunctionalInterpreter.EnumInvokeDispatcherCallResult.DONE;
                    return;
                }
                else if (path[0] == "math" && path[1] == "cos")
                {
                    Datastructures.Variadic parameterVariadic;
                    float resultFloat;

                    if (parameters.Count != 1)
                    {
                        throw new Exception("math sin required one parameters!");
                    }

                    parameterVariadic = parameters[0];

                    if (parameterVariadic.type != Datastructures.Variadic.EnumType.FLOAT)
                    {
                        throw new Exception("math cos parameter must be a float");
                    }

                    resultFloat = (float)System.Math.Cos(parameterVariadic.valueFloat);

                    result = new Datastructures.Variadic(Datastructures.Variadic.EnumType.FLOAT);
                    result.valueFloat = resultFloat;

                    callResult = ProgramRepresentation.Execution.FunctionalInterpreter.EnumInvokeDispatcherCallResult.DONE;
                    return;
                }
                else if (path[0] == "math" && path[1] == "equals")
                {
                    Datastructures.Variadic parameterA;
                    Datastructures.Variadic parameterB;
                    bool isEqual;

                    if( parameters.Count != 2 )
                    {
                        throw new Exception("math equals required two parameters!");
                    }

                    parameterA = parameters[0];
                    parameterB = parameters[1];

                    if( parameterA.type != Datastructures.Variadic.EnumType.INT && parameterA.type != Datastructures.Variadic.EnumType.FLOAT )
                    {
                        throw new Exception("math equals parameterA must be a number!");
                    }

                    if (parameterB.type != Datastructures.Variadic.EnumType.INT && parameterB.type != Datastructures.Variadic.EnumType.FLOAT)
                    {
                        throw new Exception("math equals parameterB must be a number!");
                    }

                    isEqual = Datastructures.Variadic.isEqual(parameterA, parameterB, 0.0001f);

                    result = new Datastructures.Variadic(Datastructures.Variadic.EnumType.BOOL);
                    result.valueBool = isEqual;

                    callResult = ProgramRepresentation.Execution.FunctionalInterpreter.EnumInvokeDispatcherCallResult.DONE;
                    return;
                }
                else if (path[0] == "math" && path[1] == "mod")
                {
                    Datastructures.Variadic parameterA;
                    Datastructures.Variadic parameterB;


                    if (parameters.Count != 2)
                    {
                        throw new Exception("math mod required two parameters!");
                    }

                    parameterA = parameters[0];
                    parameterB = parameters[1];

                    if (parameterA.type != Datastructures.Variadic.EnumType.INT && parameterA.type != Datastructures.Variadic.EnumType.FLOAT)
                    {
                        throw new Exception("math mod parameterA must be a number!");
                    }

                    if (parameterB.type != Datastructures.Variadic.EnumType.INT && parameterB.type != Datastructures.Variadic.EnumType.FLOAT)
                    {
                        throw new Exception("math mod parameterB must be a number!");
                    }

                    if( parameterA.type == Datastructures.Variadic.EnumType.INT && parameterB.type == Datastructures.Variadic.EnumType.INT )
                    {
                        result = new Datastructures.Variadic(Datastructures.Variadic.EnumType.INT);
                        result.valueInt = parameterA.valueInt % parameterB.valueInt;

                        callResult = ProgramRepresentation.Execution.FunctionalInterpreter.EnumInvokeDispatcherCallResult.DONE;
                        return;
                    }
                    else
                    {
                        // TODO
                        throw new Exception("TODO");
                    }
                }
                else if( path[0] == "array" && path[1] == "append" )
                {
                    Datastructures.Variadic arrayVariadic;
                    Datastructures.Variadic elementVariadic;


                    if (parameters.Count != 2)
                    {
                        throw new Exception("array append required two parameters!");
                    }

                    arrayVariadic = parameters[0];
                    elementVariadic = parameters[1];

                    if( arrayVariadic.type != Datastructures.Variadic.EnumType.ARRAY )
                    {
                        throw new Exception("array append  first parameter must be an array");
                    }

                    debug("--- array append");
                    debugVariadic("array ", arrayVariadic);
                    debugVariadic("element ", elementVariadic);


                    // NOTE< we mutate it inplace and return it >
                    arrayVariadic.valueArray.Add(elementVariadic);

                    debugVariadic("= array append result ", arrayVariadic);

                    result = arrayVariadic;

                    callResult = ProgramRepresentation.Execution.FunctionalInterpreter.EnumInvokeDispatcherCallResult.DONE;
                    return;
                }
                else if( path[0] == "array" && path[1] == "extend" )
                {
                    Datastructures.Variadic arrayVariadic;
                    Datastructures.Variadic extendVariadic;


                    if (parameters.Count != 2)
                    {
                        throw new Exception("array extend  required two parameters!");
                    }

                    arrayVariadic = parameters[0];
                    extendVariadic = parameters[1];

                    if( arrayVariadic.type != Datastructures.Variadic.EnumType.ARRAY )
                    {
                        throw new Exception("array extend  first parameter must be an array");
                    }

                    if( extendVariadic.type != Datastructures.Variadic.EnumType.ARRAY )
                    {
                        throw new Exception("array extend  second parameter must be an array");
                    }

                    // NOTE< we mutate it inplace and return it >
                    arrayVariadic.valueArray.AddRange(extendVariadic.valueArray);

                    result = arrayVariadic;

                    callResult = ProgramRepresentation.Execution.FunctionalInterpreter.EnumInvokeDispatcherCallResult.DONE;
                    return;
                }
                else if( path[0] == "array" && path[1] == "generate" )
                {
                    Datastructures.Variadic beginVariadic;
                    Datastructures.Variadic endVariadic;
                    int begin, end;
                    Datastructures.Variadic resultVariadic;
                    int i;

                    if (parameters.Count != 2)
                    {
                        throw new Exception("array generate  required two parameters!");
                    }

                    beginVariadic = parameters[0];
                    endVariadic = parameters[1];

                    if( beginVariadic.type != Datastructures.Variadic.EnumType.INT )
                    {
                        throw new Exception("array generate  first parameter must be an integer");
                    }

                    if( endVariadic.type != Datastructures.Variadic.EnumType.INT )
                    {
                        throw new Exception("array generate  second parameter must be an integer");
                    }

                    begin = beginVariadic.valueInt;
                    end = endVariadic.valueInt;

                    resultVariadic = new Datastructures.Variadic(Datastructures.Variadic.EnumType.ARRAY);
                    resultVariadic.valueArray = new List<Datastructures.Variadic>();

                    for( i = begin; i < end; i++ )
                    {
                        Datastructures.Variadic elementVariadic;

                        elementVariadic = new Datastructures.Variadic(Datastructures.Variadic.EnumType.INT);
                        elementVariadic.valueInt = i;

                        resultVariadic.valueArray.Add(elementVariadic);
                    }

                    result = resultVariadic;

                    callResult = ProgramRepresentation.Execution.FunctionalInterpreter.EnumInvokeDispatcherCallResult.DONE;
                    return;
                }
                else if( path[0] == "array" && path[1] == "length" )
                {
                    Datastructures.Variadic arrayVariadic;

                    if (parameters.Count != 1)
                    {
                        throw new Exception("array length  required one parameters!");
                    }

                    arrayVariadic = parameters[0];

                    if( arrayVariadic.type != Datastructures.Variadic.EnumType.ARRAY )
                    {
                        throw new Exception("array length  first parameter must be an array!");
                    }

                    result = new Datastructures.Variadic(Datastructures.Variadic.EnumType.INT);
                    result.valueInt = arrayVariadic.valueArray.Count;

                    callResult = ProgramRepresentation.Execution.FunctionalInterpreter.EnumInvokeDispatcherCallResult.DONE;
                    return;
                }
                else
                {
                    result = null;
                    callResult = ProgramRepresentation.Execution.FunctionalInterpreter.EnumInvokeDispatcherCallResult.PATHINVALID;
                    return;
                }
            }
            else
            {
                result = null;
                callResult = ProgramRepresentation.Execution.FunctionalInterpreter.EnumInvokeDispatcherCallResult.PATHINVALID;
                return;
            }
        }

        private static Datastructures.Variadic builtinArrayAt(List<Datastructures.Variadic> array, int index)
        {
            if( index < 0 || index > array.Count )
            {
                throw new Exception("array access out of range!");
            }

            return array[index];
        }

        private static void debugVariadic(string text, Datastructures.Variadic variadic)
        {
            string content;

            content = text + " " + debugVariadicRecursive(variadic);

            System.Console.WriteLine(content);
        }

        private static void debug(string text)
        {
            System.Console.WriteLine(text);
        }

        private static string debugVariadicRecursive(Datastructures.Variadic variadic)
        {
            string content;

            content = "";

            if (variadic.type == Datastructures.Variadic.EnumType.ARRAY)
            {
                content += "[";

                foreach (Datastructures.Variadic iteratioVariadic in variadic.valueArray)
                {
                    content += debugVariadicRecursive(iteratioVariadic) + ", ";
                }

                content += "]";
            }
            else if( variadic.type == Datastructures.Variadic.EnumType.BOOL )
            {
                if( variadic.valueBool )
                {
                    content += "true";
                }
                else
                {
                    content += "false";
                }
            }
            else if( variadic.type == Datastructures.Variadic.EnumType.FLOAT )
            {
                content += variadic.valueFloat.ToString();
            }
            else if( variadic.type == Datastructures.Variadic.EnumType.INT )
            {
                content += variadic.valueInt.ToString();
            }
            else
            {
                // TODO also

                throw new Exception("Internal Error");
            }

            return content;
        }
    }

    class Program
    {
        

        static void Main(string[] args)
        {
            // test ART2
            //NeuralNetworks.AdaptiveResonanceTheory.Test0.test0();

            // read programs and add them to the interpreter

            ProgramRepresentation.Parser.ProgramsParser programParser;
            ProgramRepresentation.Execution.FunctionalInterpreter functionalInterpreter;
            
            Datastructures.Dag<ProgramRepresentation.DagElementData> dag;
            List<ProgramRepresentation.Parser.ProgramsParser.Program> programs;

            ProgramRepresentation.Execution.FunctionalInterpreter.InterpretationState interpretationState;

            programParser = new ProgramRepresentation.Parser.ProgramsParser();

            List<string> lines;

            lines = Misc.TextFile.readLinesFromFile("C:\\Users\\r0b3\\files\\backuped\\programmierung\\c#\\ai2" + "\\" + "ai2\\ai2\\usedSrc\\functionalPrograms" + "\\" + "matrix.txt");

            programs = programParser.parse(lines);

            dag = new Datastructures.Dag<ProgramRepresentation.DagElementData>();

            interpretationState = new ProgramRepresentation.Execution.FunctionalInterpreter.InterpretationState();

            // go though each program and
            // * add it to the dag
            // * add the path and the parameters to the interpreationState so it can invoke the program later
            foreach( ProgramRepresentation.Parser.ProgramsParser.Program currentProgram in programs )
            {
                ProgramRepresentation.Parser.Functional.ParseTreeElement parseTree;
                int dagRootElementIndex;

                ProgramRepresentation.Execution.FunctionalInterpreter.InterpretationState.InvokableProgram createdProgram;

                parseTree = ProgramRepresentation.Parser.Functional.parseRecursive(currentProgram.code);
                ProgramRepresentation.Parser.ConvertTreeToDag.convertRecursive(dag, parseTree, out dagRootElementIndex);

                createdProgram = new ProgramRepresentation.Execution.FunctionalInterpreter.InterpretationState.InvokableProgram();
                createdProgram.dagIndex = dagRootElementIndex;
                createdProgram.path = currentProgram.path;

                // transcribe variablenames
                foreach( ProgramRepresentation.Parser.ProgramsParser.Parameter iterationParameter in currentProgram.parameters )
                {
                    createdProgram.variableNames.Add(iterationParameter.name);
                }

                interpretationState.invokablePrograms.Add(createdProgram);
            }

            //ProgramRepresentation.Parser.Functional.ParseTreeElement parseTree = ProgramRepresentation.Parser.Functional.parseRecursive("(fold (invoke [\"test\"] [#index]) [0 1 2])");
            //ProgramRepresentation.Parser.Functional.ParseTreeElement parseTree = ProgramRepresentation.Parser.Functional.parseRecursive("(foreach (invoke [\"test\"] [#element]) [0])");
            
            /*
            ProgramRepresentation.Parser.Functional.ParseTreeElement parseTree = ProgramRepresentation.Parser.Functional.parseRecursive("" + 
                "(pass [" + 
                "(invoke [\"math\" \"cos\"] [#rotation]) (* (invoke [\"math\" \"sin\"] [#rotation]) -1.0) 0.0 " +
                "(invoke [\"math\" \"sin\"] [#rotation]) (invoke [\"math\" \"cos\"] [#rotation])          0.0 " +
                "0.0                                 0.0                                          1.0])");
            */


            // current test
            /*
            parseTree = ProgramRepresentation.Parser.Functional.parseRecursive("" +
            "(fold " +

            "(match (invoke [\"math\" \"equals\"] [(invoke [\"math\" \"mod\"] [(- #index 1) #nth]) 0]) " +
            "true (invoke [\"array\" \"append\"] [#accu #other]) " +
            "false #accu) " +
            

            "(invoke [\"array\" \"generate\"] [#])" +
            ")"
            );
            */



            functionalInterpreter = new ProgramRepresentation.Execution.FunctionalInterpreter();
            functionalInterpreter.invokeDispatcher = new TestDispatcher();

            


            interpretationState.currentScopeLevel = 1;

            interpretationState.scopeLevels.Add(new ProgramRepresentation.Execution.FunctionalInterpreter.InterpretationState.ScopeLevel());
            interpretationState.scopeLevels.Add(new ProgramRepresentation.Execution.FunctionalInterpreter.InterpretationState.ScopeLevel());

            interpretationState.scopeLevels[0].isTerminator = true;


            /*
            interpretationState.scopeLevels[1].dagElementIndex = 0;
             * 
             * interpretationState.scopeLevels[1].variables.Add(new ProgramRepresentation.Execution.FunctionalInterpreter.InterpretationState.ScopeLevel.Variable());
            interpretationState.scopeLevels[1].variables[0].name = "input";
            interpretationState.scopeLevels[1].variables[0].value = new Datastructures.Variadic(Datastructures.Variadic.EnumType.ARRAY);
            interpretationState.scopeLevels[1].variables[0].value.valueArray = new List<Datastructures.Variadic>();
            interpretationState.scopeLevels[1].variables[0].value.valueArray.Add(new Datastructures.Variadic(Datastructures.Variadic.EnumType.FLOAT));
            interpretationState.scopeLevels[1].variables[0].value.valueArray.Add(new Datastructures.Variadic(Datastructures.Variadic.EnumType.FLOAT));
            interpretationState.scopeLevels[1].variables[0].value.valueArray.Add(new Datastructures.Variadic(Datastructures.Variadic.EnumType.FLOAT));
            interpretationState.scopeLevels[1].variables[0].value.valueArray.Add(new Datastructures.Variadic(Datastructures.Variadic.EnumType.FLOAT));
            interpretationState.scopeLevels[1].variables[0].value.valueArray[0].valueFloat = 1.0f;
            interpretationState.scopeLevels[1].variables[0].value.valueArray[1].valueFloat = 2.0f;
            interpretationState.scopeLevels[1].variables[0].value.valueArray[2].valueFloat = 3.0f;
            interpretationState.scopeLevels[1].variables[0].value.valueArray[3].valueFloat = 4.0f;

            interpretationState.scopeLevels[1].variables.Add(new ProgramRepresentation.Execution.FunctionalInterpreter.InterpretationState.ScopeLevel.Variable());
            interpretationState.scopeLevels[1].variables[1].name = "nth";
            interpretationState.scopeLevels[1].variables[1].value = new Datastructures.Variadic(Datastructures.Variadic.EnumType.INT);
            interpretationState.scopeLevels[1].variables[1].value.valueInt = 2;
            */

            /*
            interpretationState.scopeLevels[1].dagElementIndex = 123;

            interpretationState.scopeLevels[1].variables.Add(new ProgramRepresentation.Execution.FunctionalInterpreter.InterpretationState.ScopeLevel.Variable());
            interpretationState.scopeLevels[1].variables[0].name = "a";
            interpretationState.scopeLevels[1].variables[0].value = new Datastructures.Variadic(Datastructures.Variadic.EnumType.ARRAY);
            interpretationState.scopeLevels[1].variables[0].value.valueArray = new List<Datastructures.Variadic>();
            interpretationState.scopeLevels[1].variables[0].value.valueArray.Add(new Datastructures.Variadic(Datastructures.Variadic.EnumType.INT));
            interpretationState.scopeLevels[1].variables[0].value.valueArray.Add(new Datastructures.Variadic(Datastructures.Variadic.EnumType.INT));
            interpretationState.scopeLevels[1].variables[0].value.valueArray[0].valueInt = 5;
            interpretationState.scopeLevels[1].variables[0].value.valueArray[1].valueInt = 6;

            interpretationState.scopeLevels[1].variables.Add(new ProgramRepresentation.Execution.FunctionalInterpreter.InterpretationState.ScopeLevel.Variable());
            interpretationState.scopeLevels[1].variables[1].name = "b";
            interpretationState.scopeLevels[1].variables[1].value = new Datastructures.Variadic(Datastructures.Variadic.EnumType.ARRAY);
            interpretationState.scopeLevels[1].variables[1].value.valueArray = new List<Datastructures.Variadic>();
            interpretationState.scopeLevels[1].variables[1].value.valueArray.Add(new Datastructures.Variadic(Datastructures.Variadic.EnumType.INT));
            interpretationState.scopeLevels[1].variables[1].value.valueArray.Add(new Datastructures.Variadic(Datastructures.Variadic.EnumType.INT));
            interpretationState.scopeLevels[1].variables[1].value.valueArray[0].valueInt = 1;
            interpretationState.scopeLevels[1].variables[1].value.valueArray[1].valueInt = 2;
            */

            interpretationState.scopeLevels[1].dagElementIndex = 199;

            for(;;)
            {
                bool terminatorReached;

                functionalInterpreter.interpreteStep(dag, interpretationState, out terminatorReached);
                if( terminatorReached )
                {
                    break;
                }
            }
            
            
            // output gabor kernel
            /*
            {
                Map2d<float> gaborKernel = Math.GaborKernel.generateGaborKernel(64, 0.0f, 10.0f/64.0f, (float)System.Math.PI*0.5f, 0.4f);

                Bitmap outputBitmap = new Bitmap(64, 64);

                int xp;
                int yp;

                for (xp = 0; xp < 64; xp++)
                {
                    for (yp = 0; yp < 64; yp++)
                    {
                        Color color;

                        float valueFloat;
                        int valueInt;

                        valueFloat = gaborKernel.readAt(xp, yp);

                        valueFloat = System.Math.Min(1.0f, valueFloat);
                        valueFloat = System.Math.Max(0.0f, valueFloat);

                        valueInt = (int)(valueFloat * 255.0f);

                        color = Color.FromArgb(valueInt, valueInt, valueInt);

                        outputBitmap.SetPixel(xp, yp, color);
                    }
                }

                outputBitmap.Save("C:\\Users\\r0b3\\temp\\aiExperiment0\\output\\gabor.png");
            }
             */
            

            // test fft
            /*
                double[,] kernelAsArray = new double[2, 2];
                kernelAsArray[0,0] = 1.0;

                fft.FFT fftKernel = new fft.FFT(kernelAsArray);
                fftKernel.doFft(fft.FFT.EnumDirection.FORWARD);
                fft.ComplexNumber[,] fftOfKernel = fftKernel.Output;

                fft.FFT fftInverse = new fft.FFT(fftOfKernel);
                fftInverse.doFft(fft.FFT.EnumDirection.BACKWARD);
                double[,] inverseResult = fftInverse.inverseResult;

                int sdjisfdjiosfdjiosfd = 0;
            */

            // calculate gabor kernel result
            /*
            {
                Map2d<float> inputMap;
                Map2d<float> resultMap;
                ComputationBackend.cs.OperatorGaborFilter gaborFilter;

                // generate test map
                inputMap = new Map2d<float>(128, 128);
                {
                    int x, y;

                    for( x = 0; x < 64; x++ )
                    {
                        for( y = 0; y < 64; y++ )
                        {
                            inputMap.writeAt(x + 32, y + 32, 1.0f);
                        }
                    }
                }

                gaborFilter = new ComputationBackend.cs.OperatorGaborFilter();
                gaborFilter.inputMap = inputMap;

                gaborFilter.kernelLamda = 5.0f/16.0f;
                gaborFilter.kernelPhi = 0.0f;
                gaborFilter.kernelWidth = 16;

                gaborFilter.calculateKernel();

                gaborFilter.calculate(null);

                resultMap = gaborFilter.outputMap;

                Bitmap outputBitmap = new Bitmap(128, 128);

                int xp;
                int yp;

                for (xp = 0; xp < 128; xp++)
                {
                    for (yp = 0; yp < 128; yp++)
                    {
                        Color color;

                        float valueFloat;
                        int valueInt;

                        valueFloat = resultMap.readAt(xp, yp);

                        valueFloat = System.Math.Min(1.0f, valueFloat);
                        valueFloat = System.Math.Max(0.0f, valueFloat);


                        valueInt = (int)(valueFloat * 255.0f);

                        color = Color.FromArgb(valueInt, valueInt, valueInt);

                        outputBitmap.SetPixel(xp, yp, color);
                    }
                }

                outputBitmap.Save("C:\\Users\\r0b3\\temp\\aiExperiment0\\output\\gabor.png");
            }
             */

            string pathToInputImages = "C:\\Users\\r0b3\\temp\\aiExperiment0\\input\\";
            string pathToOutputImages = "C:\\Users\\r0b3\\temp\\aiExperiment0\\output\\";
            int numberOfImages = 9240;


            
            
            ////////////

            string programLocation = Assembly.GetEntryAssembly().Location;

            string pathToLoad = Path.Combine(Path.GetDirectoryName(programLocation), "..\\..\\", "usedSrc\\programs\\cauchyDistribution.txt");


            ProgramRepresentation.Program readProgram = ProgramRepresentation.ProgramReader.readProgram(pathToLoad);

            string CsSource = ProgramRepresentation.CsGenerator.generateSource(readProgram, 0, new Dictionary<string, int>());

            string realSource = string.Format("// autogenerated\n\nclass CauchyDistributionClass{{\npublic static float cauchyDistribution(float x, float s, float t)\n{{\n{0}\n}}\n}}", CsSource);

            string pathToStoreSource = Path.Combine(Path.GetDirectoryName(programLocation), "..\\..\\", "usedSrc\\generated\\Cs\\cauchyDistribution.cs");

            System.IO.File.WriteAllLines(pathToStoreSource, new String[] { realSource });



            int imageNumber;

            Random random;

            random = new Random();

            ResourceMetric metric;

            metric = new ResourceMetric();

            MainContext mainContext;
            MainContext.Configuration mainContextConfiguration;

            int radialKernelPositionsLength;

            mainContextConfiguration = new MainContext.Configuration();
            mainContextConfiguration.imageSize = new Vector2<int>();
            mainContextConfiguration.imageSize.x = 1280;
            mainContextConfiguration.imageSize.y = 720;
            mainContextConfiguration.radialKernelSize = 3;

            mainContextConfiguration.attentionDownsamplePower = 4;
            mainContextConfiguration.attentionForgettingFactor = 0.7f; // like in the paper

            radialKernelPositionsLength = ((mainContextConfiguration.imageSize.x / 4) - 1) * (mainContextConfiguration.imageSize.y / 4);
            radialKernelPositionsLength += (32 - (radialKernelPositionsLength % 32));

            mainContextConfiguration.radialKernelPositionsLength = radialKernelPositionsLength;


            mainContext = new MainContext();
            mainContext.configure(mainContextConfiguration);
            mainContext.initialize();



            // test neural stuff
            /*
            {
                NeuralNetworks.Neuroids.Neuroid<float, int> neuroidNetwork = new NeuralNetworks.Neuroids.Neuroid<float, int>();
                neuroidNetwork.update = new NeuroidModels.Test0(6, 0.5f);

                neuroidNetwork.allocateNeurons(2, 1);

                neuroidNetwork.addConnection(0, 1, 1.0f);
                
                neuroidNetwork.input = new bool[1];

                int step;

                neuroidNetwork.initialize();

                neuroidNetwork.input[0] = true;

                for (step = 0; step < 20; step++ )
                {

                    
                    neuroidNetwork.timestep();

                    neuroidNetwork.input[0] = false;

                    neuroidNetwork.debugAllNeurons();

                }


                int x = 0;
            }
            */



            // test visual system
            {
                int frameNumber;


                ComputationBackend.cs.OperatorNeuroidVision neuroidVision;

                neuroidVision = new ComputationBackend.cs.OperatorNeuroidVision();

                neuroidVision.configuration.cornerThreshold = 0.05f;
                neuroidVision.configuration.edgeThreshold = 0.05f;
                neuroidVision.configuration.imageSize = new Vector2<int>();
                neuroidVision.configuration.imageSize.x = 64;
                neuroidVision.configuration.imageSize.y = 64;
                neuroidVision.configuration.directionCount = 2; // for testing
                neuroidVision.configuration.directionSampleDistance = 2;

                
                neuroidVision.configuration.layer0NeuroidLatencyAfterFiring = 6;
                neuroidVision.configuration.layer0NeuroidRandomFiringPropability = 0.001f; // for testing

                neuroidVision.initialize(mainContext.computeContext);

                for( frameNumber = 0; frameNumber < 200; frameNumber++)
                {

                    neuroidVision.inputImage = new Map2d<float>((uint)neuroidVision.configuration.imageSize.x, (uint)neuroidVision.configuration.imageSize.y);

                    {
                        int x, y1, y;

                        for (y1 = 0; y1 < 64-20; y1+=20)
                        {
                            for (y = y1; y < y1 + 10; y++ )
                            {
                                for (x = 0; x < 64; x++)
                                {
                                    neuroidVision.inputImage.writeAt(x, y, 1.0f);
                                }
                            }

                                
                        }
                    }

                    neuroidVision.calculate(mainContext.computeContext);

                    // store image
                    Bitmap outputBitmap = new Bitmap(neuroidVision.configuration.imageSize.x, neuroidVision.configuration.imageSize.y);

                    int xp;
                    int yp;

                    for (xp = 0; xp < neuroidVision.configuration.imageSize.x; xp++)
                    {
                        for (yp = 0; yp < neuroidVision.configuration.imageSize.y; yp++)
                        {
                            Color color;
                            ColorRgb readColor;


                            readColor = neuroidVision.debugOutput.readAt(xp, yp);

                            color = Color.FromArgb((int)(readColor.r * 255.0f), (int)(readColor.g * 255.0f), (int)(readColor.b * 255.0f));

                            outputBitmap.SetPixel(xp, yp, color);
                        }
                    }

                    outputBitmap.Save(string.Format("C:\\Users\\r0b3\\temp\\aiExperiment0\\output\\visualDebug{0}.png", frameNumber));
                }


            }



            return;


            ComputationBackend.cs.ParticleMotionTracker particleMotionTracker;

            // TODO< move into main context >
            particleMotionTracker = new ComputationBackend.cs.ParticleMotionTracker();
            particleMotionTracker.initialize(mainContext.computeContext, mainContextConfiguration.imageSize);


            ComputationBackend.cs.OperatorColorTransform colorTransformForYellowBlue;
            ComputationBackend.cs.OperatorColorTransform colorTransformForRedGreen;

            colorTransformForYellowBlue = new ComputationBackend.cs.OperatorColorTransform();
            colorTransformForYellowBlue.colorForZero = new ColorRgb(0.5f, 0.5f, 0.0f);
            colorTransformForYellowBlue.colorForOne = new ColorRgb(0.0f, 0.0f, 1.0f);

            colorTransformForRedGreen = new ComputationBackend.cs.OperatorColorTransform();
            colorTransformForRedGreen.colorForZero = new ColorRgb(0.0f, 1.0f, 0.0f);
            colorTransformForRedGreen.colorForOne = new ColorRgb(1.0f, 0.0f, 0.0f);

            float repelMultiplicator = 0.01f;
            float fuseDistance = 0.05f;
            int forceIterations = 10;


            NeuralNetworks.AddaptiveParticle.AddaptiveParticle networkForImagePatches = new NeuralNetworks.AddaptiveParticle.AddaptiveParticle(repelMultiplicator, fuseDistance, forceIterations);
            //networkForImagePatches.distanceDelegate = getDistanceOfPatch;

            Map2d<float> channelRedGreen;
            Map2d<float> channelYellowBlue;

            channelRedGreen = new Map2d<float>((uint)mainContextConfiguration.imageSize.x, (uint)mainContextConfiguration.imageSize.y);
            channelYellowBlue = new Map2d<float>((uint)mainContextConfiguration.imageSize.x, (uint)mainContextConfiguration.imageSize.y);


            // test causal set algorithm
            if (false)
            {
                random = new Random(465654); // just for testing

                PartialOrderedSet.PartialOrderedSetAlgorithm causalSetAlgorithm;
                List<PartialOrderedSet.Relation> relations;

                causalSetAlgorithm = new PartialOrderedSet.PartialOrderedSetAlgorithm(random);

                relations = new List<PartialOrderedSet.Relation>();


                string line;
                StreamReader file = null;
                file = new StreamReader(/*"C:\\Users\\r0b3\\temp\\causalSet.txt"*/ /*"C:\\Users\\r0b3\\temp\\causalSetLevel1.txt"*/"C:\\Users\\r0b3\\temp\\causalSetMyExperiment2.txt");
                while ((line = file.ReadLine()) != null)
                {
                    string[] relationStrings = line.Split(new char[] { ',' });

                    foreach (string relationString in relationStrings)
                    {
                        string relationString2 = relationString.Trim();

                        if (relationString2.Length == 0)
                        {
                            continue;
                        }

                        string[] causalStrings = relationString2.Split(new char[] { '<' });

                        Console.WriteLine(causalStrings[0].Trim());
                        Console.WriteLine(causalStrings[0].Trim());

                        int number0, number1;

                        number0 = Convert.ToInt32(causalStrings[0].Trim());
                        number1 = Convert.ToInt32(causalStrings[1].Trim());

                        // order is correct
                        relations.Add(new PartialOrderedSet.Relation(number0, number1));
                    }
                }


                // translate relations to constrains
                // because the relations are index based and not the actual element values
                List<CausalSets.Constraint> causalConstraints = new List<CausalSets.Constraint>();

                foreach (PartialOrderedSet.Relation iterationRelation in relations)
                {
                    CausalSets.Constraint newConstraint;

                    // we assume here that the indices are equal to the element numbers

                    newConstraint = new CausalSets.Constraint();
                    newConstraint.preiorElement = iterationRelation.sourceIndex;
                    newConstraint.postierElement = iterationRelation.destinationIndex;

                    causalConstraints.Add(newConstraint);
                }

                causalSetAlgorithm.fillCausalRelations(/*1322*/12, relations);

                causalSetAlgorithm.work(1);

                List<int> permutation = causalSetAlgorithm.getBestPermutation();



                // reorder the preordered permutation to a better permutation
                PartialOrderedSet.NetworkAlgorithm2 causalNeuronBasedAlgorithm;

                causalNeuronBasedAlgorithm = new PartialOrderedSet.NetworkAlgorithm2();
                causalNeuronBasedAlgorithm.random = random;
                causalNeuronBasedAlgorithm.triesUntilGiveUp = 50;
                causalNeuronBasedAlgorithm.maxIterations = 500000000;
                causalNeuronBasedAlgorithm.settingCommuteOnZero = false;
                causalNeuronBasedAlgorithm.poolMaxSize = 1;
                causalNeuronBasedAlgorithm.poolInitialSize = 1;

                List<int> reorderedPermutation = causalNeuronBasedAlgorithm.doWork(causalConstraints, permutation);


                /*

                CausalSets.IterativeBlockDeeping iterativeBlockDeepeningAlgorithm;

                iterativeBlockDeepeningAlgorithm = new CausalSets.IterativeBlockDeeping();
                iterativeBlockDeepeningAlgorithm.causalNeuronBasedAlgorithm = causalNeuronBasedAlgorithm;

                List<int> reorderedPermutation = causalNeuronBasedAlgorithm.doWork(causalConstraints, permutation); //iterativeBlockDeepeningAlgorithm.doWork(permutation, causalConstraints);
                */
                int skkdkd = 0;

                return;






                List<int> resultBestPermutation;

                List<int> bestPermutation = new List<int>();

                file = new StreamReader("C:\\Users\\r0b3\\temp\\causalSetCorrectPermutation.txt");
                while ((line = file.ReadLine()) != null)
                {
                    string[] elementStrings = line.Split(new char[] { ' ' });

                    foreach (string elementString in elementStrings)
                    {
                        string trimedElementString = elementString.Trim();

                        if (trimedElementString.Length == 0)
                        {
                            continue;
                        }

                        bestPermutation.Add(Convert.ToInt32(trimedElementString));
                    }
                }

                //resultBestPermutation = bestPermutation;
                resultBestPermutation = reorderedPermutation;//permutation;//causalSetAlgorithm.getBestPermutation();

                // read point coordinates
                List<float> pointCoordinates = new List<float>();

                file = new StreamReader("C:\\Users\\r0b3\\temp\\orginalPointCoordinates.txt");
                while ((line = file.ReadLine()) != null)
                {
                    string[] elementStrings = line.Split(new char[] { ' ' });
                    List<string> nonvoidElements = new List<string>();

                    foreach (string elementString in elementStrings)
                    {
                        string trimedElementString = elementString.Trim();

                        if (trimedElementString.Length == 0)
                        {
                            continue;
                        }

                        nonvoidElements.Add(trimedElementString);
                    }

                    nonvoidElements[1] = nonvoidElements[1].Replace(".", ",");
                    nonvoidElements[2] = nonvoidElements[2].Replace(".", ",");

                    pointCoordinates.Add((float)Convert.ToDouble(nonvoidElements[1]));
                    pointCoordinates.Add((float)Convert.ToDouble(nonvoidElements[2]));
                }



                Bitmap drawingBitmap = new Bitmap(500, 500);

                Graphics drawingGraphics = Graphics.FromImage(drawingBitmap);

                Pen penRed = new Pen(Brushes.Red);
                Pen penBlue = new Pen(Brushes.Blue);

                Vector2<int> lastPointPosition;

                lastPointPosition = new Vector2<int>();
                lastPointPosition.x = 0;
                lastPointPosition.y = 0;

                // iterate throug the permutation and pick out the elements which are points, connect them with (colorcoded) lines
                int permutationIndex;
                for (permutationIndex = 0; permutationIndex < resultBestPermutation.Count; permutationIndex++)
                {
                    int permutationElement;

                    permutationElement = resultBestPermutation[permutationIndex];

                    // div by 2 because we store until now only single floats
                    if (permutationElement < pointCoordinates.Count / 2)
                    {
                        Vector2<int> pointPosition;

                        pointPosition = new Vector2<int>();
                        pointPosition.x = 0 + (int)((float)480 * (pointCoordinates[permutationElement * 2 + 0] / 10.0f));
                        pointPosition.y = 0 + (int)((float)480 * (pointCoordinates[permutationElement * 2 + 1] / 10.0f));

                        drawingGraphics.DrawLine(penBlue, lastPointPosition.x, lastPointPosition.y, pointPosition.x, pointPosition.y);

                        lastPointPosition = pointPosition;
                    }
                }

                drawingGraphics.Flush();

                drawingBitmap.Save("C:\\Users\\r0b3\\temp\\connectedPoints.png");

                int sdksfdjksfdjklsfdjkl = 0;
            }


            Map2d<ColorRgb> imageColor;
            Map2d<float> grayscaleImage;

            imageColor = new Map2d<ColorRgb>(1280, 720);


            for (imageNumber = 0; imageNumber < numberOfImages; imageNumber++)
            {
                // indicates if we need to reseed the points we track
                bool reseedTrackingPoints;






                Console.WriteLine(imageNumber.ToString());

                Stopwatch stopwatch;

                stopwatch = new Stopwatch();

                {
                    Image readImage;



                    metric.startTimer("visual", "read image", "");
                    readImage = Image.FromFile(pathToInputImages + (imageNumber + 1).ToString() + ".png");
                    metric.stopTimer();

                    // convert it to white/black image



                    Bitmap workingBitmap = new Bitmap(readImage);


                    grayscaleImage = new Map2d<float>((uint)workingBitmap.Width, (uint)workingBitmap.Height);

                    //imageRComponent = new Map2d<float>((uint)workingBitmap.Width, (uint)workingBitmap.Height);
                    //imageGComponent = new Map2d<float>((uint)workingBitmap.Width, (uint)workingBitmap.Height);
                    //imageBComponent = new Map2d<float>((uint)workingBitmap.Width, (uint)workingBitmap.Height);

                    int x, y;

                    metric.startTimer("visual", "conversion", "");

                    var bitmapData = workingBitmap.LockBits(new Rectangle(new Point(0, 0), new Size(workingBitmap.Width, workingBitmap.Height)), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    var length = bitmapData.Stride * bitmapData.Height;

                    byte[] rawImage = new byte[length];

                    // Copy bitmap to byte[]
                    Marshal.Copy(bitmapData.Scan0, rawImage, 0, length);
                    workingBitmap.UnlockBits(bitmapData);

                    for (y = 0; y < grayscaleImage.getLength(); y++)
                    {
                        for (x = 0; x < grayscaleImage.getWidth(); x++)
                        {
                            float grayscaleValue;
                            Color readColor;

                            float colorRed = (float)rawImage[x * 3 + 0 + y * bitmapData.Stride] / 255.0f;
                            float colorGreen = (float)rawImage[x * 3 + 1 + y * bitmapData.Stride] / 255.0f;
                            float colorBlue = (float)rawImage[x * 3 + 2 + y * bitmapData.Stride] / 255.0f;



                            //readColor = workingBitmap.GetPixel(x, y);

                            grayscaleValue = (colorRed + colorGreen + colorBlue/* + (float)readColor.G / 255.0f + (float)readColor.B / 255.0f*/) / 3.0f;

                            grayscaleImage.writeAt(x, y, grayscaleValue);

                            //imageRComponent.writeAt(x, y, colorRed);
                            //imageGComponent.writeAt(x, y, colorGreen);
                            //imageBComponent.writeAt(x, y, colorBlue);

                            imageColor.writeAt(x, y, new ColorRgb(colorRed, colorGreen, colorBlue));
                        }
                    }

                    metric.stopTimer();
                }

                metric.startTimer("visual", "transform color to red/green and yellow/blue", "");

                colorTransformForRedGreen.inputRgb = imageColor;
                colorTransformForRedGreen.resultMap = new Map2d<float>(1280, 720);
                colorTransformForRedGreen.calculate(mainContext.computeContext);

                channelRedGreen = colorTransformForRedGreen.resultMap;

                colorTransformForYellowBlue.inputRgb = imageColor;
                colorTransformForYellowBlue.resultMap = new Map2d<float>(1280, 720);
                colorTransformForYellowBlue.calculate(mainContext.computeContext);

                channelYellowBlue = colorTransformForYellowBlue.resultMap;

                metric.stopTimer();


                metric.startTimer("visual", "blurring", "");

                Map2d<float> bluredGrayscaleImage = new Map2d<float>(grayscaleImage.getWidth(), grayscaleImage.getLength());
                mainContext.calculateOperatorBlur(metric, grayscaleImage, bluredGrayscaleImage);

                metric.stopTimer();




                metric.startTimer("visual", "edge detect", "");

                Map2d<float> edgesAsFloat = Algorithm.Visual.EdgeDetector.detectEdgesFloat(bluredGrayscaleImage);
                metric.stopTimer();

                metric.startTimer("visual", "threshold", "");
                Map2d<bool> edgesImage2 = Algorithms.Visual.Binary.threshold(edgesAsFloat, 0.15f);
                metric.stopTimer();

                /*
                metric.startTimer("visual", "skeletalize", "");
                Map2d<bool> edgesImage = Algorithms.Visual.Binary.skeletalize(edgesImage2);
                metric.stopTimer();
                */
                Map2d<bool> edgesImage = new Map2d<bool>(edgesImage2.getWidth(), edgesImage2.getLength());
                mainContext.calculateOperatorSkeletalize(metric, edgesImage2, edgesImage);




                // estimate edges



                int numberOfAngles = 20;

                Algorithms.Visual.RadialKernelDetector radialKernelDetector;
                radialKernelDetector = new Algorithms.Visual.RadialKernelDetector();
                radialKernelDetector.configure(3);

                List<LinearBorderEntry> linearBorderEntries = new List<LinearBorderEntry>();

                for (int angleI = 0; angleI < numberOfAngles; angleI++)
                {
                    LinearBorderEntry createdLinearBorderEntry;

                    int radialKernelArrayLength = (((int)edgesImage.getWidth() / 4) - 1) * ((int)edgesImage.getLength() / 4);
                    radialKernelArrayLength += (32 - (radialKernelArrayLength % 32));

                    createdLinearBorderEntry = new LinearBorderEntry();
                    createdLinearBorderEntry.radialKernelResults = new float[radialKernelArrayLength];
                    createdLinearBorderEntry.radialKernelPositions = new Vector2<int>[radialKernelArrayLength];
                    createdLinearBorderEntry.borderCrossed = new bool[radialKernelArrayLength];

                    if (false /*angleI == 0*/ )
                    {
                        /*
                        // angle is 0 degrees

                        int xp;
                        int yp;

                        for (xp = 0; xp < (edgesImage.getWidth() / 4); xp++)
                        {
                            for (yp = 0; yp < (edgesImage.getLength() / 4); yp++)
                            {
                                createdLinearBorderEntry.radialKernelPositions[xp + (edgesImage.getWidth() / 4) * yp] = new Vector2<int>();
                                createdLinearBorderEntry.radialKernelPositions[xp + (edgesImage.getWidth() / 4) * yp].x = xp * 4;
                                createdLinearBorderEntry.radialKernelPositions[xp + (edgesImage.getWidth() / 4) * yp].y = yp * 4;
                            }
                        }

                        createdLinearBorderEntry.indexOfOtherSide = createdLinearBorderEntry.radialKernelPositions.Length;
                         */
                    }
                    else if (angleI == numberOfAngles - 1)
                    {
                        int xp;
                        int yp;

                        for (xp = 0; xp < (edgesImage.getWidth() / 4) - 1; xp++)
                        {

                            for (yp = 0; yp < (edgesImage.getLength() / 4); yp++)
                            {
                                int currentXCoordinate;
                                int currentYCoordinate;

                                currentXCoordinate = xp * 4;
                                currentYCoordinate = yp * 4;

                                createdLinearBorderEntry.radialKernelPositions[xp + ((edgesImage.getWidth() / 4) - 1) * yp] = new Vector2<int>();
                                createdLinearBorderEntry.radialKernelPositions[xp + ((edgesImage.getWidth() / 4) - 1) * yp].x = currentXCoordinate;
                                createdLinearBorderEntry.radialKernelPositions[xp + ((edgesImage.getWidth() / 4) - 1) * yp].y = currentYCoordinate;

                            }
                        }
                    }
                    else
                    {
                        // angle is between -90 and 90 degrees

                        float cosOfAngle;
                        float sinOfAngle;

                        float angle;

                        int xp;
                        int yp;

                        angle = (((float)angleI / (float)numberOfAngles) * 2.0f - 1.0f) * (float)System.Math.PI * 0.5f;

                        cosOfAngle = (float)System.Math.Cos(angle);
                        sinOfAngle = (float)System.Math.Sin(angle);
                        float tanOfAngle = sinOfAngle / cosOfAngle;

                        int previousYCoordinate;

                        for (yp = 0; yp < (edgesImage.getLength() / 4); yp++)
                        {
                            bool borderCrossed;

                            borderCrossed = false;

                            previousYCoordinate = 0;

                            // - 1 because 
                            for (xp = 0; xp < (edgesImage.getWidth() / 4) - 1; xp++)
                            {
                                int currentXCoordinate;
                                int currentYCoordinate;

                                currentXCoordinate = (int)((float)(xp * 4));
                                currentYCoordinate = (int)(((int)((float)(yp * 4) + (float)(xp * 4) * tanOfAngle)));
                                currentYCoordinate = Misc.Math.modi(currentYCoordinate, (int)edgesImage.getLength());

                                if (angle > 0.0f)
                                {
                                    if (currentYCoordinate < previousYCoordinate)
                                    {
                                        borderCrossed = true;
                                    }

                                }
                                // todo< case for negative angle

                                createdLinearBorderEntry.radialKernelPositions[xp + ((edgesImage.getWidth() / 4) - 1) * yp] = new Vector2<int>();
                                createdLinearBorderEntry.radialKernelPositions[xp + ((edgesImage.getWidth() / 4) - 1) * yp].x = currentXCoordinate;
                                createdLinearBorderEntry.radialKernelPositions[xp + ((edgesImage.getWidth() / 4) - 1) * yp].y = currentYCoordinate;

                                createdLinearBorderEntry.borderCrossed[xp + ((edgesImage.getWidth() / 4) - 1) * yp] = borderCrossed;

                                previousYCoordinate = currentYCoordinate;
                            }
                        }
                    }

                    linearBorderEntries.Add(createdLinearBorderEntry);
                }





                // test code generation
                if (true)
                {
                    /*
                    ComputationBackend.OpenCl.OperatorRadialKernel operatorRadialKernel;
                    ComputationBackend.OpenCl.ComputeContext computeContext;

                    computeContext = new ComputationBackend.OpenCl.ComputeContext();
                    computeContext.initialize();

                    operatorRadialKernel = new ComputationBackend.OpenCl.OperatorRadialKernel();
                    //operatorRadialKernel.initialize();

                    operatorRadialKernel.createKernel(1);

                    Misc.Vector2<int> imageSize;
                    imageSize = new Vector2<int>();
                    imageSize.x = 1280;
                    imageSize.y = 720;

                    int kernelPositionsLength = ((imageSize.x / 4) - 1) * (imageSize.y / 4);

                    operatorRadialKernel.initialize(computeContext, kernelPositionsLength, imageSize);

                    operatorRadialKernel.inputMap = edgesAsFloat;
                    operatorRadialKernel.kernelPositions = linearBorderEntries[0].radialKernelPositions;

                    

                    operatorRadialKernel.calculate(computeContext);

                    // pull out the result
                    operatorRadialKernel.kernelResults.CopyTo(linearBorderEntries[0].radialKernelResults, 0);
                     */
                    int i;

                    // for testing
                    /*
                    int x5, y5;
                    for( x5 = 0; x5 < edgesAsFloat.getWidth(); x5++ )
                    {
                        for( y5 = 0; y5 < edgesAsFloat.getLength(); y5++ )
                        {
                            edgesAsFloat.writeAt(x5, y5, 1.0f);
                        }
                    }*/

                    for (i = 0; i < numberOfAngles; i++)
                    {
                        mainContext.calculateRadialKernel(metric, edgesAsFloat, linearBorderEntries[i].radialKernelPositions, ref linearBorderEntries[i].radialKernelResults);
                    }
                }









                // gpu
                Vector2<int>[] inputPositionsForFindNearest = new Vector2<int>[particleMotionTracker.trackedBorderPixels.Count];
                bool[] foundNewPositionsForFindNearest = new bool[particleMotionTracker.trackedBorderPixels.Count];
                Vector2<int>[] outputPositionsFromFindNearest = new Vector2<int>[particleMotionTracker.trackedBorderPixels.Count];


                int trackedPixelI;

                // translate to
                for (trackedPixelI = 0; trackedPixelI < particleMotionTracker.trackedBorderPixels.Count; trackedPixelI++)
                {
                    inputPositionsForFindNearest[trackedPixelI] = particleMotionTracker.trackedBorderPixels[trackedPixelI].position;
                }


                mainContext.calculateOperatorFindNearest(metric, edgesImage, inputPositionsForFindNearest, ref foundNewPositionsForFindNearest, ref outputPositionsFromFindNearest);


                int resultPixelI;

                trackedPixelI = 0;

                // translate from

                for (resultPixelI = 0; resultPixelI < particleMotionTracker.trackedBorderPixels.Count; resultPixelI++)
                {
                    if (foundNewPositionsForFindNearest[resultPixelI])
                    {
                        particleMotionTracker.trackedBorderPixels[trackedPixelI].position = outputPositionsFromFindNearest[resultPixelI];

                        int xxx = 0;
                    }
                    else
                    {
                        particleMotionTracker.trackedBorderPixels.RemoveAt(trackedPixelI);
                        trackedPixelI--;
                    }

                    trackedPixelI++;
                }






                // TODO< other strategy for reseeding >


                reseedTrackingPoints = true;

                particleMotionTracker.reseedTrackingPoints(metric, mainContext.computeContext, edgesImage);

                // should stay static

                // removes tracking points which are too close together
                // algorithm uses a (at the beginning empty) bool-map2d
                // for each point we sample the bitmap, if it is void we draw a small rectangle (with the radius as boundaries)
                //                                      if it is not void we throw the sample away

                // this algorithm ensures that only the oldest points keep alive

                // algorithm outline
                // - sort each tracking point after age
                // - for each point
                //   - check if map is true
                //    - if yes, remove the point, continue
                //    - if no, draw small rectangle and continue



                {
                    Map2d<bool> usedMap;
                    int iterationTrackedPixelI;
                    List<ComputationBackend.cs.ParticleMotionTracker.TrackedPixel> sortedTrackedBorderPixels;
                    int removedTrackedPointsCounter;

                    usedMap = new Map2d<bool>(grayscaleImage.getWidth(), grayscaleImage.getLength());

                    removedTrackedPointsCounter = 0;

                    metric.startTimer("visual", "remove too close tracking points", "sort");
                    sortedTrackedBorderPixels = particleMotionTracker.trackedBorderPixels.OrderByDescending(o => o.age).ToList();
                    metric.stopTimer();

                    metric.startTimer("visual", "remove too close tracking points", "map");

                    for (iterationTrackedPixelI = 0; iterationTrackedPixelI < sortedTrackedBorderPixels.Count; iterationTrackedPixelI++)
                    {
                        bool pointToRemove;
                        ComputationBackend.cs.ParticleMotionTracker.TrackedPixel iterationTrackedPixel;

                        iterationTrackedPixel = sortedTrackedBorderPixels[iterationTrackedPixelI];

                        pointToRemove = usedMap.readAt((int)iterationTrackedPixel.position.x, (int)iterationTrackedPixel.position.y);

                        if (pointToRemove)
                        {
                            sortedTrackedBorderPixels.RemoveAt(iterationTrackedPixelI);
                            iterationTrackedPixelI--;

                            removedTrackedPointsCounter++;

                            continue;
                        }
                        else
                        {
                            // draw rectangle

                            Vector2<int> boxPosition;



                            boxPosition = new Vector2<int>();
                            boxPosition.x = (int)iterationTrackedPixel.position.x;
                            boxPosition.y = (int)iterationTrackedPixel.position.y;

                            int x, y;
                            Vector2<int> minBorder;
                            Vector2<int> maxBorder;
                            Vector2<int> boxRadius;

                            boxRadius = new Vector2<int>();
                            boxRadius.x = 2; // box radius
                            boxRadius.y = 2; // box radius

                            minBorder = new Vector2<int>();
                            minBorder.x = 0;
                            minBorder.y = 0;

                            maxBorder = new Vector2<int>();
                            maxBorder.x = (int)grayscaleImage.getWidth() - 1;
                            maxBorder.y = (int)grayscaleImage.getLength() - 1;

                            minBorder = Vector2<int>.max(minBorder, boxPosition - boxRadius);
                            maxBorder = Vector2<int>.min(maxBorder, boxPosition + boxRadius);

                            for (y = minBorder.y; y < maxBorder.y; y++)
                            {
                                for (x = minBorder.x; x < maxBorder.x; x++)
                                {
                                    usedMap.writeAt(x, y, true);
                                }
                            }
                        }
                    }

                    // we can do this because we don't depend in any way until now on the order of the pixels
                    particleMotionTracker.trackedBorderPixels = sortedTrackedBorderPixels; // = trackedBorderPixels.OrderByDescending(o => o.age).ToList();

                    Console.Write("removed tracked border pixels ");
                    Console.WriteLine(removedTrackedPointsCounter);
                }

                metric.stopTimer();





                // find neightbors of tracked pixels
                /*
                 * uncommented because it is too slow
                 * 
                 * but it works good
                {
                    stopwatch = new Stopwatch();
                    stopwatch.Start();

                    int outerIndex;

                    for( outerIndex = 0; outerIndex < trackedBorderPixels.Count; outerIndex++ )
                    {
                        int innerIndex;

                        trackedBorderPixels[outerIndex].neightborIndices.Clear();

                        for( innerIndex = 0; innerIndex < trackedBorderPixels.Count; innerIndex++ )
                        {
                            float distance;
                            
                            Vector2<int> diff;

                            diff = trackedBorderPixels[outerIndex].position - trackedBorderPixels[innerIndex].position;

                            distance = (float)Math.Sqrt(diff.x * diff.x + diff.y * diff.y);

                            // the distance calculation is a bit weird because we test against boxed distances in the algorithm to throw out points
                            if( distance < 4.0f+1.001f )
                            {
                                trackedBorderPixels[outerIndex].neightborIndices.Add(innerIndex);
                            }
                        }
                    }

                    stopwatch.Stop();
                    Console.WriteLine("neightborhoodsearch needed: {0}", stopwatch.Elapsed);
                }
                 */



                // ==================================
                // ==================================
                // attention algorithm
                // http://ilab.usc.edu/surprise/

                metric.startTimer("visual attention", "motion drawing", "");

                Bitmap motionBitmap;
                motionBitmap = new Bitmap(1280, 720);

                Graphics motionBitmapGraphics = Graphics.FromImage(motionBitmap);

                Pen whitePen = new Pen(Brushes.White);

                foreach (ComputationBackend.cs.ParticleMotionTracker.TrackedPixel iterationTrackedPixel in particleMotionTracker.trackedBorderPixels)
                {

                    if (
                        iterationTrackedPixel.position.x == iterationTrackedPixel.oldPosition.x &&
                        iterationTrackedPixel.position.y == iterationTrackedPixel.oldPosition.y
                    )
                    {
                        continue;
                    }


                    motionBitmapGraphics.DrawLine(
                        whitePen,
                        iterationTrackedPixel.position.x,
                        iterationTrackedPixel.position.y,
                        iterationTrackedPixel.oldPosition.x,
                        iterationTrackedPixel.oldPosition.y
                    );
                }

                motionBitmapGraphics.Flush();

                metric.stopTimer();


                metric.startTimer("visual attention", "convert motion to map", "");

                Map2d<float> motionMap;

                motionMap = new Map2d<float>((uint)motionBitmap.Width, (uint)motionBitmap.Height);

                float[] motionMapArray = motionMap.unsafeGetValues();


                var bitmapData2 = motionBitmap.LockBits(new Rectangle(new Point(0, 0), new Size(motionBitmap.Width, motionBitmap.Height)), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                var length2 = bitmapData2.Stride * bitmapData2.Height;

                byte[] rawImage2 = new byte[length2];

                // Copy bitmap to byte[]
                Marshal.Copy(bitmapData2.Scan0, rawImage2, 0, length2);
                motionBitmap.UnlockBits(bitmapData2);


                int x2, y2;

                for (x2 = 0; x2 < motionMap.getWidth(); x2++)
                {
                    for (y2 = 0; y2 < motionMap.getLength(); y2++)
                    {
                        float value;

                        value = (float)rawImage2[x2 * 3 + bitmapData2.Stride * y2] / 255.0f;//)(float)(motionBitmap.GetPixel(x2, y2).R / 255);
                        motionMapArray[x2 + y2 * motionMap.getWidth()] = value;
                        //motionMap.writeAt(x2, y2, value);
                    }
                }


                metric.stopTimer();

                /*
                metric.startTimer("visual attention", "blur motion map", "");

                Map2d<float> bluredMotionMap = new Map2d<float>(motionMap.getWidth(), motionMap.getLength());

                ComputationBackend.OpenCl.OperatorBlur blurMotionMap;
                blurMotionMap = new ComputationBackend.OpenCl.OperatorBlur();
                blurMotionMap.inputMap = motionMap;
                blurMotionMap.outputMap = bluredMotionMap;

                Vector2<int> blurMapSize = new Vector2<int>();
                blurMapSize.x = (int)motionMap.getWidth();
                blurMapSize.y = (int)motionMap.getLength();

                blurMotionMap.initialize(mainContext.computeContext, 5, blurMapSize);

                blurMotionMap.calculate(mainContext.computeContext);

                metric.stopTimer();
                */
                mainContext.calculateAttentionModule(metric, motionMap);

                // TODO< other stuff necessary >



                // now we try to remember small patches around the attention center
                // this gets later used by the whole higher visual stuff
                Vector2<int> positionOfMostAttention = mainContext.getAttentionModule().getPositionOfMostAttention();

                int visualPatchSize = 32; // 32 pixel is the size of each patch

                int numberOfPatchesAroundAttention = 10; // must be even

                metric.startTimer("visual", "segment and store patches", "");

                Vector2<int> visualPatchBegin;
                Vector2<int> visualPatchEnd;

                Vector2<int> visualPatchMin;
                Vector2<int> visualPatchMax;

                visualPatchMin = new Vector2<int>();
                visualPatchMax = new Vector2<int>();

                visualPatchMin.x = 0;
                visualPatchMin.y = 0;

                visualPatchMax.x = 1280;
                visualPatchMax.y = 720;

                Vector2<int> halfPatchWidth;

                halfPatchWidth = new Vector2<int>();
                halfPatchWidth.x = (numberOfPatchesAroundAttention / 2) * visualPatchSize;
                halfPatchWidth.y = (numberOfPatchesAroundAttention / 2) * visualPatchSize;

                visualPatchBegin = positionOfMostAttention - halfPatchWidth;
                visualPatchEnd = positionOfMostAttention + halfPatchWidth;

                visualPatchBegin = Vector2<int>.max(visualPatchMin, visualPatchBegin);
                visualPatchEnd = Vector2<int>.min(visualPatchMax, visualPatchEnd);

                visualPatchBegin.x = visualPatchBegin.x - (visualPatchBegin.x % visualPatchSize);
                visualPatchBegin.y = visualPatchBegin.y - (visualPatchBegin.y % visualPatchSize);

                // NOTE< this does mean that the bottom/left most border are invisible for the visual system >
                visualPatchEnd.x = visualPatchEnd.x - (visualPatchEnd.x % visualPatchSize);
                visualPatchEnd.y = visualPatchEnd.y - (visualPatchEnd.y % visualPatchSize);

                int patchX, patchY;

                float[] patchDataYellowBlue;
                float[] patchDataRedGreen;

                int lastNeuronIndex;

                lastNeuronIndex = 0;

                patchDataYellowBlue = new float[visualPatchSize * visualPatchSize];
                patchDataRedGreen = new float[visualPatchSize * visualPatchSize];

                for (patchX = visualPatchBegin.x / visualPatchSize; patchX < visualPatchEnd.x / visualPatchSize; patchX++)
                {
                    for (patchY = visualPatchBegin.y / visualPatchSize; patchY < visualPatchEnd.y / visualPatchSize; patchY++)
                    {
                        int patchI;
                        int x, y;

                        patchI = 0;

                        for (y = patchY * visualPatchSize; y < (patchY + 1) * visualPatchSize; y++)
                        {
                            for (x = patchX * visualPatchSize; x < (patchX + 1) * visualPatchSize; x++)
                            {
                                patchDataRedGreen[patchI] = channelRedGreen.readAt(x, y);
                                patchDataYellowBlue[patchI] = channelYellowBlue.readAt(x, y);

                                patchI++;
                            }
                        }

                        float[] patchVector = new float[2 * visualPatchSize * visualPatchSize];

                        int i;

                        for (i = 0; i < visualPatchSize * visualPatchSize; i++)
                        {
                            patchVector[i] = patchDataRedGreen[i];
                        }


                        for (i = 0; i < visualPatchSize * visualPatchSize; i++)
                        {
                            patchVector[i + visualPatchSize * visualPatchSize] = patchDataYellowBlue[i];
                        }

                        int neuronIndex;

                        networkForImagePatches.remember(patchVector, out neuronIndex);

                        lastNeuronIndex = neuronIndex;

                    }
                }


                metric.stopTimer();


                System.Console.WriteLine(lastNeuronIndex);







                // store image for testing
                motionBitmap.Save(pathToOutputImages + "m" + (imageNumber + 1).ToString() + ".png");

                // ==================================
                // ==================================



                // calculate (relative) velocity of tracked border pixels
                foreach (ComputationBackend.cs.ParticleMotionTracker.TrackedPixel iterationTrackedPixel in particleMotionTracker.trackedBorderPixels)
                {
                    Vector2<int> absoluteVelocity;
                    Vector2<float> velocity;

                    absoluteVelocity = iterationTrackedPixel.position - iterationTrackedPixel.oldPosition;

                    velocity = new Vector2<float>();
                    velocity.x = (float)absoluteVelocity.x / grayscaleImage.getWidth();
                    velocity.y = (float)absoluteVelocity.y / grayscaleImage.getWidth();

                    iterationTrackedPixel.velocity = velocity;

                    iterationTrackedPixel.oldPosition = iterationTrackedPixel.position.clone();
                }




                // first we group the points and then we cluster the groups
                // after that we try to fuse different clusters

                // we ignore the velocity of the points in our clustering process

                // translate the tracked points to points in the treenode
                Datastructures.TreeNode pointsTreeNode;

                pointsTreeNode = new Datastructures.TreeNode();

                foreach (ComputationBackend.cs.ParticleMotionTracker.TrackedPixel iterationTrackedPixel in particleMotionTracker.trackedBorderPixels)
                {
                    Vector2<float> normalizedPosition;
                    Datastructures.TreeNode pointTreeNode;

                    pointTreeNode = new Datastructures.TreeNode();
                    pointTreeNode.value = new Datastructures.Variadic(Datastructures.Variadic.EnumType.VECTOR2FLOAT);

                    normalizedPosition = iterationTrackedPixel.getNormalizedPosition((int)grayscaleImage.getWidth());
                    pointTreeNode.value.valueVector2Float = normalizedPosition;

                    pointsTreeNode.childNodes.Add(pointTreeNode);
                }

                Datastructures.Variadic pointsVariadic;
                pointsVariadic = new Datastructures.Variadic(Datastructures.Variadic.EnumType.STORAGEALGORITHMICCONCEPTTREE);
                pointsVariadic.valueTree = pointsTreeNode;


                Operators.Visual.GroupPointsGrid groupPointsGrid;
                groupPointsGrid = new Operators.Visual.GroupPointsGrid();
                groupPointsGrid.sizeY = (float)grayscaleImage.getLength() / (float)grayscaleImage.getWidth();

                Vector2<int> clusterWidth;
                clusterWidth = new Vector2<int>();
                clusterWidth.x = 180;
                clusterWidth.y = 180;

                groupPointsGrid.cellSize = new Vector2<float>();
                groupPointsGrid.cellSize.x = 1.0f / (float)clusterWidth.x;
                groupPointsGrid.cellSize.y = 1.0f / (float)clusterWidth.y;

                groupPointsGrid.cellOffset = new Vector2<float>();

                List<Datastructures.Variadic> parameters;
                parameters = new List<Datastructures.Variadic>();
                parameters.Add(pointsVariadic);

                Datastructures.Variadic resultGroupedPoints;

                groupPointsGrid.initialize();

                GeneticProgramming.TypeRestrictedOperator.EnumResult calleeResult;

                groupPointsGrid.call(parameters, out resultGroupedPoints, out calleeResult);


                // call again with a 50% shifted grid
                groupPointsGrid.cellOffset.x = (1.0f / (float)clusterWidth.x) * 0.5f;
                groupPointsGrid.cellOffset.y = (1.0f / (float)clusterWidth.y) * 0.5f;


                Datastructures.Variadic resultGroupedPointsShifted;

                groupPointsGrid.call(parameters, out resultGroupedPointsShifted, out calleeResult);



                Operators.Visual.ConnectGroups connectGroups;

                connectGroups = new Operators.Visual.ConnectGroups();
                connectGroups.gridsize = new Vector2<int>();
                connectGroups.gridsize.x = clusterWidth.x;
                connectGroups.gridsize.y = clusterWidth.y;

                List<Datastructures.Variadic> parametersVariadic;

                parametersVariadic = new List<Datastructures.Variadic>();
                parametersVariadic.Add(new Datastructures.Variadic(Datastructures.Variadic.EnumType.STORAGEALGORITHMICCONCEPTTREE));
                parametersVariadic.Add(new Datastructures.Variadic(Datastructures.Variadic.EnumType.STORAGEALGORITHMICCONCEPTTREE));

                parametersVariadic[0].valueTree = resultGroupedPoints.valueTree;
                parametersVariadic[1].valueTree = resultGroupedPoints.valueTree;

                Datastructures.Variadic connectedGroupsGraphVariadic;

                connectGroups.maxConnectionDistance = 0.011f * 0.85f;

                connectGroups.call(parametersVariadic, null, out connectedGroupsGraphVariadic, out calleeResult);
                System.Diagnostics.Debug.Assert(calleeResult == GeneticProgramming.TypeRestrictedOperator.EnumResult.OK);


                // call the operator for finding edges
                Operators.Visual.SearchScaffoldInGraph searchScaffoldInGraph;
                Datastructures.Variadic searchScaffoldResultVariadic;

                searchScaffoldInGraph = new Operators.Visual.SearchScaffoldInGraph();
                searchScaffoldInGraph.random = random;
                searchScaffoldInGraph.searchingScaffoldRoot = new Scaffolds.Graph.ExtractLineScaffold();

                parametersVariadic = new List<Datastructures.Variadic>();
                parametersVariadic.Add(new Datastructures.Variadic(Datastructures.Variadic.EnumType.STORAGEALGORITHMICCONCEPTTREE));

                parametersVariadic[0] = connectedGroupsGraphVariadic;

                stopwatch.Restart();

                //searchScaffoldInGraph.call(parametersVariadic, null, out searchScaffoldResultVariadic, out calleeResult);

                stopwatch.Stop();
                //Console.WriteLine("=visual   find lines              took {0} ms", stopwatch.Elapsed.Milliseconds);

                System.Diagnostics.Debug.Assert(calleeResult == GeneticProgramming.TypeRestrictedOperator.EnumResult.OK);



                // add one to age of particles
                foreach (ComputationBackend.cs.ParticleMotionTracker.TrackedPixel iterationTrackedPixel in particleMotionTracker.trackedBorderPixels)
                {
                    iterationTrackedPixel.age++;
                }


                // drawing

                // just for testing


                stopwatch = new Stopwatch();
                stopwatch.Start();

                if (true)
                {
                    Image readImage;

                    readImage = Image.FromFile(pathToInputImages + (imageNumber + 1).ToString() + ".png");

                    // convert the image for showing to HSL and draw only the H component
                    Bitmap bitmapX = new Bitmap(readImage);

                    for (int y = 0; y < bitmapX.Height; y++)
                    {
                        for (int x = 0; x < bitmapX.Width; x++)
                        {
                            Color readColor = bitmapX.GetPixel(x, y);

                            ColorRgb readRgb = new ColorRgb((float)readColor.R / 255.0f, (float)readColor.G / 255.0f, (float)readColor.B / 255.0f);

                            ColorHsl hsl = ColorConversion.rgbToHsl(readRgb);

                            Color writeColor;

                            /*
                            if (edgesImage.readAt(x, y))
                            {
                                writeColor = Color.FromArgb(0, 0, 255);
                            }
                            else
                            {
                                writeColor = Color.FromArgb((int)(hsl.l * 255.0f), (int)(hsl.h * 255.0f), (int)(hsl.h * 255.0f));
                            }
                             * */

                            float readEdgeValue = edgesAsFloat.readAt(x, y);

                            int grayscaleValue = (int)((readEdgeValue / 10.0f) * 255.0f);

                            grayscaleValue = System.Math.Min(grayscaleValue, 255);

                            writeColor = Color.FromArgb(grayscaleValue, grayscaleValue, grayscaleValue);

                            bitmapX.SetPixel(x, y, writeColor);
                        }
                    }

                    bitmapX.Save(pathToOutputImages + "h" + (imageNumber + 1).ToString() + ".png");

                }

                // draw the result into a new image and store it in the output folder
                {
                    Image readImage;

                    readImage = Image.FromFile(pathToInputImages + (imageNumber + 1).ToString() + ".png");

                    // convert it to white/black image


                    Graphics drawingGraphics = Graphics.FromImage(readImage);

                    Pen penRed = new Pen(Brushes.Red);
                    Pen penGreen = new Pen(Brushes.Green);
                    Pen penYellow = new Pen(Brushes.Yellow);
                    Pen penWhite = new Pen(Brushes.White);

                    /*
                    foreach( TrackedPixel iterationTrackedPixel in trackedPixels )
                    {
                        drawingGraphics.DrawLine(penRed, iterationTrackedPixel.position.x, (int)iterationTrackedPixel.position.y - 3, iterationTrackedPixel.position.x, iterationTrackedPixel.position.y + 3);
                        drawingGraphics.DrawLine(penRed, (int)iterationTrackedPixel.position.x - 3, iterationTrackedPixel.position.y, iterationTrackedPixel.position.x + 3, iterationTrackedPixel.position.y);
                    }
                     */


                    /*
                    // draw neightbor connections
                    foreach (TrackedPixel iterationTrackedPixel in trackedBorderPixels)
                    {
                        foreach(int neightborIndex in iterationTrackedPixel.neightborIndices)
                        {
                            Vector2<int> neightborPosition;

                            neightborPosition = trackedBorderPixels[neightborIndex].position;

                            drawingGraphics.DrawLine(penGreen, iterationTrackedPixel.position.x, iterationTrackedPixel.position.y, neightborPosition.x, neightborPosition.y);
                        }
                    }
                     */



                    if (false)
                    {
                        foreach (ComputationBackend.cs.ParticleMotionTracker.TrackedPixel iterationTrackedPixel in particleMotionTracker.trackedBorderPixels)
                        {
                            drawingGraphics.DrawLine(penRed, iterationTrackedPixel.position.x, (int)iterationTrackedPixel.position.y - 3, iterationTrackedPixel.position.x, iterationTrackedPixel.position.y + 3);
                            drawingGraphics.DrawLine(penRed, (int)iterationTrackedPixel.position.x - 3, iterationTrackedPixel.position.y, iterationTrackedPixel.position.x + 3, iterationTrackedPixel.position.y);
                        }

                    }

                    // draw point groups

                    if (false)
                    {
                        foreach (Datastructures.TreeNode groupTreeNode in resultGroupedPoints.valueTree.childNodes)
                        {
                            Vector2<float> middle;
                            float radius;
                            Datastructures.TreeNode groupElementsTreeNode;

                            groupElementsTreeNode = groupTreeNode.childNodes[0];

                            middle = new Vector2<float>();

                            radius = 0.0f;

                            // calculate middlepoint

                            foreach (Datastructures.TreeNode pointTreeNode in groupElementsTreeNode.childNodes)
                            {
                                middle += pointTreeNode.value.valueVector2Float;
                            }

                            middle.scale(1.0f / (float)groupElementsTreeNode.childNodes.Count);

                            // calculate radius

                            foreach (Datastructures.TreeNode pointTreeNode in groupElementsTreeNode.childNodes)
                            {
                                float iterationPointDistance;

                                iterationPointDistance = (pointTreeNode.value.valueVector2Float - middle).magnitude();

                                if (iterationPointDistance > radius)
                                {
                                    radius = iterationPointDistance;
                                }

                            }

                            // draw it
                            int circlePositionX = (int)(middle.x * (float)grayscaleImage.getWidth());
                            int circlePositionY = (int)(middle.y * (float)grayscaleImage.getWidth());
                            int circleRadius = (int)(radius * (float)grayscaleImage.getWidth());

                            drawingGraphics.DrawEllipse(penGreen, circlePositionX - circleRadius, circlePositionY - circleRadius, circleRadius * 2, circleRadius * 2);
                        }



                        foreach (Datastructures.TreeNode groupTreeNode in resultGroupedPointsShifted.valueTree.childNodes)
                        {
                            Vector2<float> middle;
                            float radius;
                            Datastructures.TreeNode groupElementsTreeNode;

                            groupElementsTreeNode = groupTreeNode.childNodes[0];

                            middle = new Vector2<float>();

                            radius = 0.0f;

                            // calculate middlepoint

                            foreach (Datastructures.TreeNode pointTreeNode in groupElementsTreeNode.childNodes)
                            {
                                middle += pointTreeNode.value.valueVector2Float;
                            }

                            middle.scale(1.0f / (float)groupElementsTreeNode.childNodes.Count);

                            // calculate radius

                            foreach (Datastructures.TreeNode pointTreeNode in groupElementsTreeNode.childNodes)
                            {
                                float iterationPointDistance;

                                iterationPointDistance = (pointTreeNode.value.valueVector2Float - middle).magnitude();

                                if (iterationPointDistance > radius)
                                {
                                    radius = iterationPointDistance;
                                }

                            }

                            // draw it
                            int circlePositionX = (int)(middle.x * (float)grayscaleImage.getWidth());
                            int circlePositionY = (int)(middle.y * (float)grayscaleImage.getWidth());
                            int circleRadius = (int)(radius * (float)grayscaleImage.getWidth());

                            drawingGraphics.DrawEllipse(penYellow, circlePositionX - circleRadius, circlePositionY - circleRadius, circleRadius * 2, circleRadius * 2);
                        }
                    }


                    // draw connected egde groups/(edges between edge point groups)
                    Datastructures.TreeNode connectedEdgeGroupsGraph;
                    connectedEdgeGroupsGraph = connectedGroupsGraphVariadic.valueTree;

                    foreach (Datastructures.TreeNode graphEdgeTreeNode in connectedEdgeGroupsGraph.childNodes[1].childNodes)
                    {
                        int graphVertexAIndex;
                        int graphVertexBIndex;

                        graphVertexAIndex = graphEdgeTreeNode.childNodes[0].value.valueInt;
                        graphVertexBIndex = graphEdgeTreeNode.childNodes[1].value.valueInt;

                        Vector2<int> middleA;
                        Vector2<int> middleB;

                        middleA = new Vector2<int>();
                        middleB = new Vector2<int>();

                        // calculate middle for A
                        {
                            int vertexIndex = graphVertexAIndex;

                            Datastructures.TreeNode graphVertices;

                            graphVertices = connectedEdgeGroupsGraph.childNodes[0];


                            Datastructures.TreeNode groupElementsTreeNode;

                            groupElementsTreeNode = graphVertices.childNodes[vertexIndex].childNodes[0];

                            Vector2<float> middle;
                            middle = new Vector2<float>();

                            // calculate middlepoint

                            foreach (Datastructures.TreeNode pointTreeNode in groupElementsTreeNode.childNodes)
                            {
                                middle += pointTreeNode.value.valueVector2Float;
                            }

                            middle.scale(1.0f / (float)groupElementsTreeNode.childNodes.Count);

                            middleA.x = (int)(middle.x * (float)grayscaleImage.getWidth());
                            middleA.y = (int)(middle.y * (float)grayscaleImage.getWidth());
                        }

                        // calculate middleB
                        {
                            int vertexIndex = graphVertexBIndex;

                            Datastructures.TreeNode graphVertices;

                            graphVertices = connectedEdgeGroupsGraph.childNodes[0];


                            Datastructures.TreeNode groupElementsTreeNode;

                            groupElementsTreeNode = graphVertices.childNodes[vertexIndex].childNodes[0];

                            Vector2<float> middle;
                            middle = new Vector2<float>();

                            // calculate middlepoint

                            foreach (Datastructures.TreeNode pointTreeNode in groupElementsTreeNode.childNodes)
                            {
                                middle += pointTreeNode.value.valueVector2Float;
                            }

                            middle.scale(1.0f / (float)groupElementsTreeNode.childNodes.Count);

                            middleB.x = (int)(middle.x * (float)grayscaleImage.getWidth());
                            middleB.y = (int)(middle.y * (float)grayscaleImage.getWidth());
                        }

                        // draw line
                        drawingGraphics.DrawLine(penWhite, middleA.x, middleA.y, middleB.x, middleB.y);
                    }

                    // draw found lines
                    /*
                    foreach( Operators.Visual.SearchScaffoldInGraph.Line iterationLine in searchScaffoldInGraph.lines )
                    {
                        Vector2<int> a;
                        Vector2<int> b;

                        a = new Vector2<int>();
                        b = new Vector2<int>();

                        a.x = (int)( iterationLine.a.x * (float)grayscaleImage.getWidth() );
                        a.y = (int)( iterationLine.a.y * (float)grayscaleImage.getWidth() );

                        b.x = (int)(iterationLine.b.x * (float)grayscaleImage.getWidth());
                        b.y = (int)(iterationLine.b.y * (float)grayscaleImage.getWidth());

                        drawingGraphics.DrawLine(penGreen, a.x, a.y, b.x, b.y);
                    }*/

                    // estimate edges and draw if they are above threshold
                    int i;

                    for (i = 0; i < numberOfAngles; i++)
                    {
                        int xp;
                        int yp;

                        for (xp = 1; xp < (edgesImage.getWidth() / 4) - 1; xp++)
                        {
                            for (yp = 0; yp < (edgesImage.getLength() / 4); yp++)
                            {
                                float strength;

                                strength = 0.0f;
                                strength += linearBorderEntries[i].radialKernelResults[(xp - 1) + ((edgesImage.getWidth() / 4) - 1) * yp];
                                strength += linearBorderEntries[i].radialKernelResults[(xp) + ((edgesImage.getWidth() / 4) - 1) * yp];
                                strength += linearBorderEntries[i].radialKernelResults[(xp + 1) + ((edgesImage.getWidth() / 4) - 1) * yp];

                                if (strength > 5.5f)
                                {
                                    Vector2<int> lineBegin;
                                    Vector2<int> lineEnd;

                                    if (linearBorderEntries[i].radialKernelPositions[xp + 1 + ((edgesImage.getWidth() / 4) - 1) * yp] == null)
                                    {
                                        continue;
                                    }

                                    lineBegin = linearBorderEntries[i].radialKernelPositions[xp - 1 + ((edgesImage.getWidth() / 4) - 1) * yp];
                                    lineEnd = linearBorderEntries[i].radialKernelPositions[xp + 1 + ((edgesImage.getWidth() / 4) - 1) * yp];


                                    drawingGraphics.DrawLine(penGreen, lineBegin.x, lineBegin.y, lineEnd.x, lineEnd.y);
                                }
                            }
                        }
                    }


                    // draw line clusters

                    /*
                    int clusterColorCounter = 0;
                    foreach( Algorithms.Visual.PointToLineSegmentation.ClusteredTrackedPixels iterationCluster in clusteredTrackedPixels )
                    {
                        Pen penCluster;
                        Vector2<int> lastPoint;

                        if( (clusterColorCounter % 2) == 0 )
                        {
                           penCluster = new Pen(Brushes.Yellow);
                        }
                        else
                        {
                            penCluster = new Pen(Brushes.Gray);
                        }

                        lastPoint = iterationCluster.trackedPixels[0].position;

                        foreach( TrackedPixel iterationPixel in iterationCluster.trackedPixels )
                        {
                            drawingGraphics.DrawLine(penCluster, lastPoint.x, lastPoint.y, iterationPixel.position.x, iterationPixel.position.y);

                            lastPoint = iterationPixel.position;
                        }

                        clusterColorCounter++;
                    }
                     * */

                    drawingGraphics.Flush();

                    //Bitmap resultBitmap = GraphicsBitmapConverter.GraphicsToBitmap(drawingGraphics, Rectangle.Truncate(drawingGraphics.VisibleClipBounds));

                    //resultBitmap.Save(pathToOutputImages + (imageNumber + 1).ToString() + ".png");

                    readImage.Save(pathToOutputImages + (imageNumber + 1).ToString() + ".png");
                }


                // draw attention map
                {
                    Map2d<float> novelityMap;
                    Bitmap outputBitmap;

                    novelityMap = mainContext.attentionModuleGetMasterMap();

                    outputBitmap = new Bitmap((int)novelityMap.getWidth(), (int)novelityMap.getLength());

                    int xp;
                    int yp;

                    for (xp = 1; xp < novelityMap.getWidth(); xp++)
                    {
                        for (yp = 0; yp < novelityMap.getLength(); yp++)
                        {
                            Color color;

                            float valueFloat;
                            int valueInt;

                            valueFloat = novelityMap.readAt(xp, yp);

                            valueFloat = System.Math.Min(valueFloat, 1.0f);
                            valueFloat /= 1.0f;

                            valueInt = (int)(valueFloat * 255.0f);

                            color = Color.FromArgb(valueInt, valueInt, valueInt);

                            outputBitmap.SetPixel(xp, yp, color);
                        }
                    }

                    outputBitmap.Save(pathToOutputImages + "a" + (imageNumber + 1).ToString() + ".png");
                }

                stopwatch.Stop();

                Console.WriteLine("writing needed: {0}",
                    stopwatch.Elapsed);

                metric.report();
                metric.reset();
            }
        }
    }

}