﻿using System;

namespace Math
{
    sealed class Gamma
    {
        // code is from mrob.com/pub/ries/lanczos-gamma.html
        // LICENSE UNKNOWN

        // The following constants LG_g and LG_N are the "g" and "n" parameters
        // for the table of coefficients that follows them; several alternative
        // coefficients are available at mrob.com/pub/ries/lanczos-gamma.html
        private const double LG_g = 5.0; // Lanczos parameter "g"
        private static readonly double[] LanczosConstants = {
            1.000000000190015,
            76.18009172947146,
            -86.50532032941677,
            24.01409824083091,
            -1.231739572450155,
            0.1208650973866179e-2,
            -0.5395239384953e-5
        };

        private const double LNSQRT2PI = 0.91893853320467274178;


        // Compute the logarithm of the Gamma function using the Lanczos method.
        static public double lanczosLnGamma(double z)
        {
            double sum;
            double baseValue;
            int i;

            if( z < 0.5 )
            {
                // Use Euler's reflection formula:
                // Gamma(z) = Pi / [Sin[Pi*z] * Gamma[1-z]];
                return System.Math.Log(System.Math.PI / System.Math.Sin(System.Math.PI * z)) - lanczosLnGamma(1.0 - z);
            }
            
            z = z - 1.0;
            baseValue = z + LG_g + 0.5;  // Base of the Lanczos exponential
            sum = 0;

            // We start with the terms that have the smallest coefficients and largest
            // denominator.
            i = LanczosConstants.Length-1;
            for( ; i >= 1; i-- )
            {
                sum += LanczosConstants[i] / (z + ((double)i));
            }
            sum += LanczosConstants[0];

            // This printf is just for debugging
            //printf("ls2p %7g  l(b^e) %7g   -b %7g  l(s) %7g\n", LNSQRT2PI, log(base)*(z+0.5), -base, log(sum));
            
            // Gamma[z] = Sqrt(2*Pi) * sum * base^[z + 0.5] / E^base
            return ((LNSQRT2PI + System.Math.Log(sum)) - baseValue) + System.Math.Log(baseValue)*(z+0.5);
        }

        // Compute the Gamma function, which is e to the power of ln_gamma.
        static public double lanczosGamma(double z)
        {
            return System.Math.Exp(lanczosLnGamma(z));
        }

        // src http://web.science.mq.edu.au/~mjohnson/code/digamma.c
        // LICENSE UNKNOWN
        static public double diGamma(double x) {
            double result = 0, xx, xx2, xx4;

            System.Diagnostics.Debug.Assert(x > 0.0);
            
            for ( ; x < 7; ++x)
            {
                result -= 1/x;
            }
            
            x -= 1.0/2.0;
            xx = 1.0/x;
            xx2 = xx*xx;
            xx4 = xx2*xx2;
            result += System.Math.Log(x)+(1.0/24.0)*xx2-(7.0/960.0)*xx4+(31.0/8064.0)*xx4*xx2-(127.0/30720.0)*xx4*xx4;
            return result;
        }
    }
}
