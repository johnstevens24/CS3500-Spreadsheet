//Author: John Stevens CS3500 FA21
//Based on the skeleton code written by Joe Zachary and updated by Daniel Kopta

using System;
using System.Collections;
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
        private Hashtable dependents; //list of nodes that depend on this node
        private Hashtable dependees; //list of nodes that this node depends on
        

        /// <summary>
        /// Creates an empty DependencyGraph.
        /// </summary>
        public DependencyGraph()
        {
            dependees = new Hashtable(); 
            dependents = new Hashtable();
        }


        /// <summary>
        /// The number of ordered pairs in the DependencyGraph.
        /// </summary>
        public int Size
        {
            get
            {
                int count = 0;
                ICollection values = dependees.Values;
                foreach (ArrayList list in values)
                    count += list.Count;

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
                if (dependees.Contains(s))
                {  
                    ArrayList list = (ArrayList)dependees[s];
                    return list.Count;
                }
                else
                    return 0;
            }
        }


        /// <summary>
        /// Reports whether dependents(s) is non-empty.
        /// </summary>
        public bool HasDependents(string s)
        {
            if (dependents.ContainsKey(s) == false)
                return false;
            else
            {
                ArrayList list = (ArrayList)dependents[s];
                if (list.Count == 0 || list.Equals(null))
                    return false;
                return true;
            }
        }


        /// <summary>
        /// Reports whether dependees(s) is non-empty.
        /// </summary>
        public bool HasDependees(string s)
        {
            if (dependees.ContainsKey(s) == false)
                return false;
            else
            {
                ArrayList list = (ArrayList)dependees[s];
                if (list.Count == 0 || list.Equals(null))
                    return false;
                return true;
            }
        }


        /// <summary>
        /// Enumerates dependents(s).
        /// </summary>
        public IEnumerable<string> GetDependents(string s)
        {
            if (dependents.ContainsKey(s))
            {
                ArrayList list = (ArrayList)dependents[s];
                String[] dependentsList = new String[list.Count];

                for (int i = 0; i < list.Count; i++)
                    dependentsList[i] = (string)list[i];

                return dependentsList;
            }
            return Enumerable.Empty<string>();
        }

        /// <summary>
        /// Enumerates dependees(s).
        /// </summary>
        public IEnumerable<string> GetDependees(string s)
        {            
            if(dependees.ContainsKey(s))
            {
                ArrayList list = (ArrayList)dependees[s];
                String[] dependeesList = new String[list.Count];

                for (int i = 0; i < list.Count; i++)
                    dependeesList[i] = (string)list[i];

                return dependeesList;
                }
            return Enumerable.Empty<string>();
        }


        /// <summary>
        /// <para>Adds the ordered pair (s,t), if it doesn't exist</para>
        /// 
        /// <para>This should be thought of as:</para>   
        /// 
        ///  t depends on s
        ///
        /// </summary>
        /// <param name="s"> s must be evaluated first. T depends on S</param>
        /// <param name="t"> t cannot be evaluated until s is</param>        /// 
        public void AddDependency(string s, string t)
        {
            //add dependent
            ArrayList list;
            if (dependents.ContainsKey(s) == false)
            {
                list = new ArrayList();
                list.Add(t);
                dependents.Add(s, list);
            }
            else
            {
            list = (ArrayList)dependents[s];
                if (list.Contains(t) == false)
                {
                    list.Add(t);
                    dependents[s] = list;
                }
            }

            //update dependee list
            if (dependees.ContainsKey(t) == false)
            {
                list = new ArrayList();
                list.Add(s);
                dependees.Add(t, list);
            }
            else
            {
                list = (ArrayList)dependees[t];
                if (list.Contains(s) == false)
                {
                    list.Add(s);
                    dependees[t] = list;
                }
            }

        }


        /// <summary>
        /// Removes the ordered pair (s,t), if it exists
        /// </summary>
        /// <param name="s">the first value of the ordered pair</param>
        /// <param name="t">the second value of the ordered pair</param>
        public void RemoveDependency(string s, string t)
        {
            //remove dependent
            if(dependents.ContainsKey(s))
            { 
                ArrayList list = (ArrayList)dependents[s];
                if(list.Contains(t))
                {
                    list.Remove(t);
                    dependents[s] = list;                    
                }
                if (!HasDependents(s))  //if it no longer has dependents, remove it from the hashtable
                    dependents.Remove(s);
            }

            //update the dependees list
            if (dependees.ContainsKey(t))
            {
                ArrayList list = (ArrayList)dependees[t];
                if (list.Contains(s))
                {
                    list.Remove(s);
                    dependees[t] = list;
                }
                if (!HasDependees(t))   //if it no longer has dependees, remove it from the hashtable
                    dependees.Remove(t);
            }

        }


        /// <summary>
        /// Removes all existing ordered pairs of the form (s,r).  Then, for each
        /// t in newDependents, adds the ordered pair (s,t).
        /// </summary>
        public void ReplaceDependents(string s, IEnumerable<string> newDependents)
        {
            ArrayList list = new ArrayList();

            //need to remove s as a dependee from all its old dependents
            IEnumerable<string> oldDependee = this.GetDependents(s);
            foreach (String t in oldDependee)
            {
                if (dependees.Contains(t))
                {
                    list = (ArrayList)dependees[t];
                    if (list != null)
                    {
                        list.Remove(s);
                        dependees[t] = list;
                    }
                }
            }

            
            //compile a list of new dependents
            list = new ArrayList();
            foreach (String t in newDependents)
            {
                if (!list.Contains(t)) //prevents duplicates
                    list.Add(t);
            }

            if (dependents.ContainsKey(s) == false)
            {
                dependents.Add(s, list);
            }
            else
                dependents[s] = list;

            
            //add s as a dependee to all of the new dependents
            foreach (String t in newDependents)
            {
                list = new ArrayList();
                if(dependees.Contains(t))
                {
                   list = (ArrayList)dependees[t]; 
                   list.Add(s);
                   dependees[t] = list;
                }
                else
                {
                    list = new ArrayList();
                    list.Add(s);
                    dependees.Add(t, list);
                }
            }

        }


        /// <summary>
        /// Removes all existing ordered pairs of the form (r,s).  Then, for each 
        /// t in newDependees, adds the ordered pair (t,s).
        /// </summary>
        public void ReplaceDependees(string s, IEnumerable<string> newDependees)
        {
            ArrayList list = new ArrayList();

            //need to remove s as a dependent from all its old dependees
            IEnumerable<string> oldDependent = this.GetDependees(s);
            foreach (String t in oldDependent)
            {
                if (dependents.Contains(t))
                {
                    list = (ArrayList)dependents[t];
                    if (list != null)
                    {
                        list.Remove(s);
                        dependents[t] = list;
                    }
                }
            }

            //compile a list of new dependees
            list = new ArrayList();
            foreach (String t in newDependees)
            {
                if(!list.Contains(t)) //prevents duplicates
                list.Add(t);
            }

            if (dependees.ContainsKey(s) == false)
            {
                dependees.Add(s, list);
            }
            else
                dependees[s] = list;

            
            //add s as a dependent to all of the new dependees
            foreach (String t in newDependees)
            {
                list = new ArrayList();
                if (dependents.Contains(t))
                {
                    list = (ArrayList)dependents[t];
                    list.Add(s);
                    dependents[t] = list;
                }
                else
                {
                    list = new ArrayList();
                    list.Add(s);
                    dependents.Add(t, list);
                }
            }
        }  
    }
}
