using System;
using System.Collections.Generic;

namespace NeuralNetworks.AdaptiveResonanceTheory
{
    /**
     * code from https://web.archive.org/web/20120109162743/http://users.visualserver.org/xhudik/art
     * 
     */
    class Common
    {
        /**
         * 
         * returns -1 if it was not found
         */
        static public int instanceInSequence(List<List<int>> prototypesSequence, int instanceI)
        {
            int i, j, prototypeNumber;

            prototypeNumber = -1;
	        
            for( i = 0; i < prototypesSequence.Count; i++ )
            {
		        // every prototype is compound of different number of instances 
                for (j = 0; j < prototypesSequence[i].Count; j++ )
                {
			        if( prototypesSequence[i][j] == instanceI )
                    {
				        prototypeNumber=i;
				        break;
				    }
		        }
                
                if( prototypeNumber != -1 )
                {
                    break;
                }
	        }
	        
            return prototypeNumber;
        }

        /**
         * find a particular instance(example Ek) or prototype in a sequence
         * and give as a result the index of instance(prototype) in the sequence
         * item means instance or prototype.
         * If must_find is set up as true and the example (item) has not been found -- write error 
         * message and stop the program.
         * It is used by ART 1, ART 2A and ART 2A-C algorithms
         **/
        static public int findItem(List<Math.DynamicVector<float>> samples, Math.DynamicVector<float> instance, bool mustFind)
        {
            int index, i;

            index = -1;

            for( i = 0; i < samples.Count; i++ )
            {
                if( samples[i] == instance )
                {
                    index = i;
                    break;
                }
            }

            if( !mustFind ) 
            {
                return index;
            }
            else
            {
                if( index != -1 )
                {
                    return index;
                }
                else
                {
                    throw new Exception("sample ... was not found in sequence.");
                }
            }
        }
    }
}
