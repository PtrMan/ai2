using System;
using System.Collections.Generic;

namespace NeuralNetworks.AdaptiveResonanceTheory
{
    /**
     * code from https://web.archive.org/web/20120109162743/http://users.visualserver.org/xhudik/art
     * 
     * 
     */
    class AdaptiveResonanceTheory2
    {
        
        /**
         * Count score (similarity) how similar inst and prot are.
         * The output (similarity) will be their dot product 
         * \param inst it is some instance (example, Ek)
         * \param prot it is some prototype
         **/
        private static float countScore(Math.DynamicVector<float> prototype, Math.DynamicVector<float> instance)
        {
            float score;
            int i;

            score = 0.0f;

            for( i = 0; i < prototype.array.Length; i++ )
            {
                score += instance[i]*prototype[i];
            }

            return score;
        }

        /**
         * Add an example (Ek) to a particular cluster.
         * It means that it moves the prototype toward the example.
         * The prototype will be more similar with the example.
         * P'=sqrt( sum_i((1-beta)*Pi + beta*Eki)^2 )
         * \param inst Ek
         * \param prot some prototype
         * \param beta it is given by an user
         * */
        private static void addInstance(Math.DynamicVector<float> instance, Math.DynamicVector<float> prototype, float beta)
        {
            Math.DynamicVector<float> temp;
            float norm;
            int i;

            System.Diagnostics.Debug.Assert(beta <= 0.0f && beta <= 1.0f);
            System.Diagnostics.Debug.Assert(instance.array.Length == prototype.array.Length);

            norm = 0.0f;
            temp = new Math.DynamicVector<float>(prototype.array.Length);

            // make vector  tmp=(1-beta)*P + beta*Ek
            for( i = 0; i < instance.array.Length; i++ )
            {
                temp[i] = (1.0f-beta)*prototype[i] + beta*instance[i];
            }
            
            // count vector norm semi = sqrt(tmp^2)
	        for( i = 0; i < instance.array.Length; i++ )
            {
                norm += temp[i]*temp[i];
            }
            
            norm = (float)System.Math.Sqrt(norm);
            System.Diagnostics.Debug.Assert(norm != 0.0f);
            norm = 1.0f/norm;

            // count prototype
	        for( i = 0; i < instance.array.Length; i++ )
            {
                prototype[i] = norm*temp[i];
            }
        }

        /**
         * Removing an instance(Ek) from the particular prototype.
         * Remove the instance with index 'iinst' in 'sample' from prototype
         * with index 'iprot' in 'prot'. But also remove particular index 
         * from prototype sequence
         **/
        private static void removeInstance(List<Math.DynamicVector<float>> sample, int iinst, List<Math.DynamicVector<float>> prot, int iprot, List<List<int>> seq, float beta, float vigilance)
        {
            int i;

	        // find and erase in the prototype sequence the instance which should be deleted
	        for( i = 0; i < seq[iprot].Count; i++ )
            {
                if(seq[iprot][i]==iinst)
                {
                    seq[iprot].RemoveAt(i);
		            break;
                }
	        }

	        // if the particular prototype is empty now - delete whole prototype
	        // delete also line (prototype) in prototype sequence
	        if(seq[iprot].Count == 0)
            {
                prot.RemoveAt(iprot);
                seq.RemoveAt(iprot);
	        }
	        // if it is not empty - re-create it from the rest examples
	        else
            {
                float score;

		        // build prototype but without instance which should be deleted
		        // at first -- prototype is the first item in the prototype sequence
		        prot[iprot] = sample[seq[iprot][0]];
			    
		        // if PE < vigilance -- it won't stop (infinite looping)
		        score = countScore(sample[seq[iprot][0]], sample[seq[iprot][0]]);
		        if(score < vigilance)
                {
			        float tmpv = vigilance;
			        vigilance = score;
			        //cerr << "\nWARNING: vigilance is too high (" << tmpv << "). What means infinite looping!!!\n";
			        //cerr << "Vigilance was decreased: vigilance=" << vigilance << endl;
		        }

		        // continually add others examples
		        // it started from 2nd member because the first is already in
		        for( i = 1; i < seq[iprot].Count; i++ )
                {
		            addInstance(sample[seq[iprot][i]], prot[iprot], beta);
		        }
		
	        }
        }

        /**
         * Create a new prototype and also create a new sequence in prot_seq
         * One line in prot_seq = one cluster represented by a prototype
         * \param inst set of all examples
         * \param iinst ID of particular Ek
         * \param prot set of prototypes
         * \param prot_seq set of all prototypes with indexes of member's Ek
         * \param vigilance it is set by an user
         * */
        private static void createPrototype(List<Math.DynamicVector<float>> inst, int iinst, List<Math.DynamicVector<float>> prot, List<List<int>> prot_seq, float vigilance)
        {
	        float score;
            List<int> new_seq;

	        // if PE < vigilance -- it won't stop
	        score = countScore(inst[iinst], inst[iinst]);
	        if(score < vigilance)
            {
	            float tmpv;
	            if( (score-vigilance) < 0.0001f )
                {
		            tmpv = vigilance;
		            vigilance = vigilance - (0.0001f + 0.0001f*vigilance);
	            }
                else
                { 
		            tmpv = vigilance;
		            vigilance = score;
	            }

		        //    cerr << "\nWARNING: vigilance is too high (" << tmpv << "). What means infinite looping!!!\n";
		        //    cerr << "Vigilance was decreased: vigilance=" << vigilance << endl;
                int x = 0; // for breakpoint
	        }
	
	        // create a new prototype
	        prot.Add(inst[iinst]);
	        // create a new prototype sequence and insert the first index of instance
            new_seq = new List<int>();
	        new_seq.Add(iinst);
	        prot_seq.Add(new_seq);
        }

        
        /** 
         * Returns a prototype with highest similarity (score) -- which was not used yet.
         * The score is counted for a particular instance Ek and all the prototypes.
         * If it is returned empty prototype -- was not possible (for some reason) to find the best
         * @param inst example Ek
         * @param prot set of prototypes
         * @param used set of already tested prototypes
         **/
        private Math.DynamicVector<float> bestPrototype2A(Math.DynamicVector<float> inst, List<Math.DynamicVector<float>> prot, List<Math.DynamicVector<float>> used)
        {
            // prototypes with the same score
            List<Math.DynamicVector<float>> sameScore;
            Math.DynamicVector<float> empty;
            int usize;
            int psize;
            float[] score;
            int i, j;
            float higher;

            sameScore = new List<Math.DynamicVector<float>>();
            empty = new Math.DynamicVector<float>(0); // ASK< is size 0 right? >

            usize = used.Count;
            psize = prot.Count;


            // if the number of already used prototypes and the number of
            //  prototypes are the same return empty protot. (no best protot.)
            if( used.Count==prot.Count )
            {
                return empty;
            }

            score = new float[psize];
            // setting initial value(the minimum for type double for this particular architecture) for scoring prototypes
            for( i = 0; i < psize; i++ )
            {
                score[i] = float.MinValue;
            }

            // set score for every prototype
            for( i = 0; i < psize; i++ )
            {
                bool usedb;
                // search if prototype is not among already used prototypes
	            usedb=false;
	            for( j = 0; j < usize; j++ )
                {
	                if( prot[i] == used[j] )
                    {
      		            usedb=true;
      		            break;
	                }
                }
                
                // is proto[i] among the used ??
                if( usedb )
                {
                    continue;
                }
                // if not count it's score
                else
                {
                    score[i] = countScore(prot[i], inst);
                }
            }

            //find prototype with highest score
            higher = float.MinValue;

            for( i = 0; i < psize; i++ )
            {
   	            if( score[i] == higher )
                {
   	                sameScore.Add(prot[i]);
   	            }
   	            else
                {
	                if( score[i] > higher )
                    {
   		                // erase the old list
                        sameScore.Clear();
   		                sameScore.Add(prot[i]);
   		                higher = score[i];
   	                }
                }
            }
   
            if( sameScore.Count == 0 )
            {
                // the result is an empty prototype
                return empty;
            }
            else if( sameScore.Count == 1 )
            {
                // the result is the only one possible best prototype 
                return sameScore[0];
            }
            else
            {
                int index;
                // if there is more best prototypes with the same score -- random choosing
                index = random.Next(sameScore.Count);
   
                return sameScore[index];
            }
        }










        
    /**
     * In the structure Clust are stored all the results.<br>
     * More specifically: prototypes, fluctuation (error) and sequence
     * of all examples for each prototype
     **/
    public class Clust
    {
        /** 
         * proto is a set of created prototypes
         */
        public List<Math.DynamicVector<float>> proto;

       /** 
        * proto_seq it is a sequence of sequences (a matrix). Where each line
        * represents one prototype and each column in the line represents some
        * example's ID.<br>
        * Example:<br>
        * 1 2 4<br>
        * 7 3 5
        *<br><br>
        * The first prototype consists of the ID's examples: 1, 2 and 3<br>
        * The second cluster consist of the following examples: 7, 3 and 5<br>
        * An example with ID 5 is a vector. In fact, it is an input line
        */
        public List<List<int>> proto_seq;
 
       /** 
        * How many examples
        * were re-assign (they are in a different cluster then they were before) 
        * */
        public float fluctuation;
       };
   


        
    /**
     * In this structure are stored all input parameters important for run every art algorithm. 
     * They can be given by user or they are set as default
     **/
    public class in_param
    {
       /** 
        * Input parameter beta (-b) in ART 1 is a small positive integer. It influences a number of created clusters. The higher value the 
        * higher number of created clusters. The default is 1.<br>
        * For another ART implementations (based on real value input) <em>beta</em> is a learning constant. It has a range [0, 1]. The default
        * is 0.5
        * */
       public float beta;

      /**
       * positive integer or 0 - skip the last n columns (in input examples) -- default value:0
       * */
       int skip;


       /**
        * Input parameter vigilance (-v) together with alpha (ART for real numbers input) or beta (ART 1) set up a similarity threshold. 
        * This threshold influence a minimum similarity under which an example Ek will be accepted by prototype. The higher value 
        * the higher number of clusters. It has a range [0,1]. The default is 0.1.
        * */
       public float vigilance;

       /** 
        * Input parameter theta (-t) denoising parameter. If a value Ek_i (Ek is a example and
        * it's ith column) is lower than theta then the number will be changed to 0. It is used only by ART 2A. It's range:
        * [0,1/dim^-0.5]. Default 0.00001
        * */
       float theta = 0.00001f;

       /** 
        * Input parameter alpha (-a)  is used by real value ART algorithms. Together with vigilance set up a similarity threshold. 
        * This threshold influences a minimum similarity which is necessary for the example Ek to be accepted by the prototype. 
        * The range: [0,1/sqrt(dim)] where dim is a number of dimensions. The default is 1/sqrt(dim) * 1/2
        * */
       public float alpha;

       /** 
        * Input parameter distance (-d) set up a distance measure:
        *   <ol><li> Euclidean distance
        *    <li>Modified Euclidean distance -- it is in a testing mode. Euclidean distance use equation 1 - E/dim where E is Euclidean distance
        *    and dim is a number of dimensions. Modified Euclidean use equation log(dim^2) - E. This distance in some cases can achieve a better
        *    performance than standard Euclidean distance. However, it is recommended to use standard Euclidean distance. DO NOT USE IT
        *    <li> Manhattan distance
        *    <li> Correlation distance
        *    <li> Minkowski distance
        *   </ol>
        *    It works only for art_distance. Default Euclidean distance measure
        **/ 
       int distance;

       /** 
        * Input parameter power (-p) it is used only for Minkowski distance in art_distance. It set up the power for Minkowski 
        * distance measure. The default is 3. Minkowski with the power 1 is Manhattan distance. Minkowski with power 2 is 
        * Euclidean distance
        * */
       int power;

       /**
        * An input parameter --  a number of passes (-E), it is a maximum number of how many times an example Ek 
        * can be re-assigned. If it reach this number the program will stop. The default is 100
        * */
       public int pass;

       /** 
        * An input parameter -- fluctuation (-e), it is a highest possible error rate (%). It means a maximum
        * number (in %) of how many instances can be re-assign. If the real fluctuatio is lower than -e
        * then program will stop. Default is 5% examples.
        * */
       public float error;
    };
   


        
        /**
         * ART 2A algorithm, inputs: examples and input parameters given by an user
         * How exactly it is working can be found at www.fi.muni.cz/~xhudik/art/drafts
         * \param sample  set if input examples (Eks)
         * \param par all input parameters set by an user or default
         **/
        public void art2A(List<Math.DynamicVector<float>> sample, in_param param, Clust results)
        {
            // prototype with highest score
            Math.DynamicVector<float> P;

            // list of all prototypes
            List<Math.DynamicVector<float>> prot;

            // the best representation of the prototypes of the whole history
            List<Math.DynamicVector<float>> prot_best;

            // sequences of samples Ek from which prototype has been created
            // it is possible to reconstruct a prototype from the sequence
            // defined in art_common.h
            List<List<int>> prot_seq;

            // the best representation of the prototypes of the whole history
            List<List<int>> prot_seq_best;

            // list of prototypes which were used already
            List<Math.DynamicVector<float>> used;

            used = new List<Math.DynamicVector<float>>();
            prot = new List<Math.DynamicVector<float>>();
            prot_seq = new List<List<int>>();
            prot_best = new List<Math.DynamicVector<float>>();
            prot_seq_best = new List<List<int>>();
            
            float fluctuation = 100.0f;
            
            // the lowest error of the whole history
            // it is initialized as some impossible number(higher than 100% can't be), to avoid problems with first iteration
            float fluctuation_best = 120.0f;  

            // how many times it run throughout the samples
            int pass = 0;

            // how many Ek's has been reassign to other cluster (prototype) in a previous pass (run)
            List<bool> changed;

            int i, j;
        
            changed = new List<bool>();
            for( i = 0; i < sample.Count; i++ )
            {
                changed.Add(true);
            }

            // do cycle while error is higher than the parameter -e  or a number of passes is lower than the parameter -E
            while((pass<param.pass)&&(fluctuation>param.error))
            {
                int number_changed;

                // nullifying changed values
                for( i = 0; i < sample.Count; i++ )
                {
                    changed[i] = false;
                }

	            // cycle for instances
                for( i = 0; i < sample.Count; i++ )
                {
                    // zeroing 'used' prototypes
                    used.Clear();

                    do
                    {
                        float score;
                        float alphaSum;

	                    // find the best prototype for current Ek
                        P = bestPrototype2A(sample[i],prot,used);

                        // if there is no best prototype 
                        if( P.array.Length == 0 )
                        {
                            int prototypeIndex;

		                    //check if the instance is not included already in some other prototype
                            prototypeIndex = Common.instanceInSequence(prot_seq,i);
		                    if( prototypeIndex != -1 )
                            { 
			                    //if so, remove it (recreate prototype--without the instance)
			                    removeInstance(sample, i, prot, prototypeIndex, prot_seq, param.beta, param.vigilance);
		                    }
                	        
                            createPrototype(sample, i, prot, prot_seq, param.vigilance);
                	        changed[i] = true;
                	        break;
                        }

	                    // add P among 'used' 
		                used.Add(P);
		
		                //count similarity between P and Ek (it is called "score") and alpha*sum_i Eki
		                score = countScore(P, sample[i]);
		                alphaSum = 0.0f;
		                for( j = 0; j < sample[i].array.Length; j++ )
                        {
                            alphaSum += param.alpha*sample[i][j];
                        }

	                    // if similarity is sufficient -- sample[i] is member of the P
		                if( score >= alphaSum )
                        {
		                    if( score >= param.vigilance )
                            {
		                        int prot_index;
                                int Pindex;

			                    // if the example Ek is already included in some prototype -- find it
                  	            prot_index = Common.instanceInSequence(prot_seq,i);
                  	            if( prot_index != -1 )
                                {
			                        // test if the found prototype is not actual one (P) in that case try - go for another Ek
                                    if( prot[prot_index] == P )
                                    {
				                        break;
                                    }
                                    else
                                    {
				                        // re-build prototype - without the sample
                                        removeInstance(sample,i,prot,prot_index,prot_seq,param.beta,param.vigilance);
				                    }
                                }
                            
                                // find an index of P in prototypes
                                Pindex = Common.findItem(prot, P, true);

                                // add instance to the current prototype
                                addInstance(sample[i],prot[Pindex],param.beta);
                                prot_seq[Pindex].Add(i);
                                changed[i]=true;
                                break;
		                    }

		                    // try other best P
                            else
                            {
                                continue;
                            }
		                } //score=>alphaSize
                        else
                        {
                            int prot_index;

                            // if prototype is not enough similar to the example(sample[i]) then create a new prototype
                            
                            // check if the instance is not already in some other prototype
                            prot_index=Common.instanceInSequence(prot_seq,i);
                            if( prot_index != -1 )
                            { 
		                        // if so, remove it (recreate prototype--without the instance)
                                removeInstance(sample,i,prot,prot_index,prot_seq,param.beta,param.vigilance);
		                    }
		                    
                            createPrototype(sample,i,prot, prot_seq,param.vigilance);
		                    changed[i] = true;
		                    break;
                        }
                    }
                    while( prot.Count != sample.Count );

                } // for sample

	            //count statistics for this pass
	            number_changed=0;
	            for( j = 0; j < changed.Count; j++ )
                {
                    if( changed[j] )
                    {
                        number_changed++;
                    }
                }
	            fluctuation = ((float)number_changed/sample.Count)*100;

                pass++;

	            //cout << "Pass: " << pass <<", fluctuation: " << fluctuation << "%" << ", clusters: " << prot.size() << endl;
		
	            //test if this iteration has not lower error
	            if(fluctuation < fluctuation_best){
	   
	               //if it is so - assign the new best results
	               prot_best = prot;
	               prot_seq_best = prot_seq;
	               fluctuation_best = fluctuation;
                   }

            } // while

            // create results
            results.proto = prot_best;
            results.proto_seq = prot_seq_best;
            results.fluctuation  = fluctuation_best;
        }

        private Random random;
    }
}
