// Skeleton implementation written by Joe Zachary for CS 3500, September 2013.
// Version 1.1 (Fixed error in comment for RemoveDependency.)
// Version 1.2 - Daniel Kopta 
//               (Clarified meaning of dependent and dependee.)
//               (Clarified names in solution/project structure.)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpreadsheetUtilities
{

    /// <summary>
    /// (s1,t1) is an ordered pair of strings
    /// t1 depends on s1; s1 must be evaluated before t1
    /// 
    /// A DependencyGraph can be modeled as a set of ordered pairs of strings.  Two ordered pairs
    /// (s1,t1) and (s2,t2) are considered equal if and only if s1 equals s2 and t1 equals t2.
    /// Recall that sets never contain duplicates.  If an attempt is made to add an element to a 
    /// set, and the element is already in the set, the set remains unchanged.
    /// 
    /// Given a DependencyGraph DG:
    /// 
    ///    (1) If s is a string, the set of all strings t such that (s,t) is in DG is called dependents(s).
    ///        (The set of things that depend on s)    
    ///        
    ///    (2) If s is a string, the set of all strings t such that (t,s) is in DG is called dependees(s).
    ///        (The set of things that s depends on) 
    //
    // For example, suppose DG = {("a", "b"), ("a", "c"), ("b", "d"), ("d", "d")}
    //     dependents("a") = {"b", "c"}
    //     dependents("b") = {"d"}
    //     dependents("c") = {}
    //     dependents("d") = {"d"}
    //     dependees("a") = {}
    //     dependees("b") = {"a"}
    //     dependees("c") = {"a"}
    //     dependees("d") = {"b", "d"}
    /// </summary>
    public class DependencyGraph
    {
        private Dictionary<string, HashSet<string>> dpd;
        private Dictionary<string, HashSet<string>> dpe;

        /// <summary>
        /// Creates an empty DependencyGraph.
        /// </summary>
        public DependencyGraph()
        {
            dpd = new();
            dpe = new();
        }


        /// <summary>
        /// The number of ordered pairs in the DependencyGraph.
        /// </summary>
        public int Size
        {
            get
            {
                int count = 0;
                foreach(HashSet<string> hs in dpd.Values)
                {
                    foreach(string s in hs)
                    {
                        count++;
                    }
                }
                return count;
            }
        }


        /// <summary>
        /// The size of dependees(s).
        /// This property is an example of an indexer.  If dg is a DependencyGraph, you would
        /// invoke it like this:
        /// dg["a"]
        /// It should return the size of dependees("a")
        /// </summary>
        public int this[string s]
        {
            get
            {
                HashSet<string>? h;
                dpe.TryGetValue(s, out h);

                if (h == null)
                    return 0;
                else
                    return h.Count;
            }
        }


        /// <summary>
        /// Reports whether dependents(s) is non-empty.
        /// </summary>
        public bool HasDependents(string s)
        {
            return dpd.ContainsKey(s);
        }


        /// <summary>
        /// Reports whether dependees(s) is non-empty.
        /// </summary>
        public bool HasDependees(string s)
        {
            return dpe.ContainsKey(s);
        }


        /// <summary>
        /// Enumerates dependents(s).
        /// </summary>
        public HashSet<string> GetDependents(string s)
        {
            HashSet<string>? h;
            dpd.TryGetValue(s, out h);
            if (h == null)
                return new HashSet<string>();
            return h;
        }

        /// <summary>
        /// Enumerates dependees(s).
        /// </summary>
        public HashSet<string> GetDependees(string s)
        {
            HashSet<string>? h;
            dpe.TryGetValue(s, out h);
            if (h == null)
                return new HashSet<string>();
            return h;
        }


        /// <summary>
        /// <para>Adds the ordered pair (s,t), if it doesn't exist</para>
        /// 
        /// <para>This should be thought of as:</para>   
        /// 
        ///   t depends on s
        ///
        /// </summary>
        /// <param name="s"> s must be evaluated first. T depends on S</param>
        /// <param name="t"> t cannot be evaluated until s is</param>        /// 
        public void AddDependency(string s, string t)
        {
            //Add Dependent
            HashSet<string> dpdu = GetDependents(s);
            if(!dpdu.Contains(t))
                dpdu.Add(t);
            dpd.Remove(s);
            dpd.TryAdd(s, dpdu);

            //Add Dependee
            HashSet<string> dpeu = GetDependees(t);
            if(!dpeu.Contains(s))
                dpeu.Add(s);
            dpe.Remove(t);
            dpe.Add(t, dpeu);
        }


        /// <summary>
        /// Removes the ordered pair (s,t), if it exists
        /// </summary>
        /// <param name="s"></param>
        /// <param name="t"></param>
        public void RemoveDependency(string s, string t)
        {
            //Remove Dependent
            HashSet<string> dpdu = GetDependents(s);
            if (dpdu.Contains(t))
                dpdu.Remove(t);
            dpd.Remove(s);
            if(dpdu.Count != 0)
                dpd.Add(s, dpdu);

            //Remove Dependee
            HashSet<string> dpeu = GetDependees(t);
            if (dpeu.Contains(s))
                dpeu.Remove(s);
            dpe.Remove(t);
            if(dpeu.Count != 0)
                dpe.Add(t, dpeu);
        }


        /// <summary>
        /// Removes all existing ordered pairs of the form (s,r).  Then, for each
        /// t in newDependents, adds the ordered pair (s,t).
        /// </summary>
        public void ReplaceDependents(string s, IEnumerable<string> newDependents)
        {
            HashSet<string> currentDpd = GetDependents(s);
            foreach(string d in currentDpd)
            {
                RemoveDependency(s, d);
            }
            foreach(string d in newDependents)
            {
                AddDependency(s, d);
            }
        }


        /// <summary>
        /// Removes all existing ordered pairs of the form (r,s).  Then, for each 
        /// t in newDependees, adds the ordered pair (t,s).
        /// </summary>
        public void ReplaceDependees(string s, IEnumerable<string> newDependees)
        {
            HashSet<string> currentDpe = GetDependees(s);
            foreach(string d in currentDpe)
            {
                RemoveDependency(d, s);
            }
            foreach(string d in newDependees)
            {
                AddDependency(d, s);
            }
        }

    }

}