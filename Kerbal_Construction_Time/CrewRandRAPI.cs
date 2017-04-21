﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;

// Include this file in your project in order to support a soft-dependency on CrewRandR.
// Do not edit this file.
// Example usage: CrewRandR.API.SuppressCrew()
namespace CrewRandR
{
    public class API
    {
        private static bool? _available = null;
        private static Type _type = null;

        /// <summary>
        /// This indicates if CrewRandR is loaded
        /// </summary>
        public static bool Available
        {
            get
            {
                if (_available == null)
                {
                    _type = AssemblyLoader.loadedAssemblies
                                          .Select(a => a.assembly.GetTypes())
                                          .SelectMany(t => t)
                                          .FirstOrDefault(t => t.FullName.Equals("CrewRandR.CrewRandRProxy"));

                    _available = _type != null;
                }
                return (bool)_available;
            }
        }

        /// <summary>
        /// Returns the Kerbals who are allowed to go on missions, unsorted.
        /// </summary>
        public static IEnumerable<ProtoCrewMember> AvailableCrew
        {
            get
            {
                return (IEnumerable<ProtoCrewMember>)getProperty("AvailableCrew");
            }
        }

        /// <summary>
        /// Returns the Kerbals who are not allowed to go on missions.
        /// </summary>
        public static IEnumerable<ProtoCrewMember> UnavailableCrew
        {
            get
            {
                return (IEnumerable<ProtoCrewMember>)getProperty("UnavailableCrew");
            }
        }

        /// <summary>
        /// Returns the Kerbals who are allowed to go on missions, sorted inexperienced first.
        /// </summary>
        public static IOrderedEnumerable<ProtoCrewMember> LeastExperiencedCrew
        {
            get
            {
                return (IOrderedEnumerable<ProtoCrewMember>)getProperty("LeastExperiencedCrew");
            }
        }

        /// <summary>
        /// Returns the Kerbals who are allowed to go on missions, sorted veterans first.
        /// </summary>
        public static IOrderedEnumerable<ProtoCrewMember> MostExperiencedCrew
        {
            get
            {
                return (IOrderedEnumerable<ProtoCrewMember>)getProperty("MostExperiencedCrew");
            }
        }

        /// <summary>
        /// Obtains a group of crew for the specified part
        /// </summary>
        /// <param name="partPrefab">A reference to the Part in question.</param>
        /// <param name="preferVeterans">Check if veterans should be prioritized over newbies</param>
        /// <returns></returns>
        public IEnumerable<ProtoCrewMember> GetCrewForPart(Part partPrefab, IEnumerable<ProtoCrewMember> exemptList, bool preferVeterans = false)
        {
            return (IEnumerable<ProtoCrewMember>)invokeMethod("GetCrewForPart", new object[] { partPrefab, exemptList, preferVeterans });
        }

        // Generic accessors
        internal static object getProperty(string name, object[] indexes = null)
        {
            if (Available)
            {
                System.Reflection.PropertyInfo _property = _type.GetProperty(name, BindingFlags.Public | BindingFlags.Static);
                return _property.GetValue(null, indexes);
            }
            else
            {
                throw new Exception("Attempted to access CrewRandR without that mod installed.");
            }
        }

        internal static object invokeMethod(string name, object[] parameters = null)
        {
            if (Available)
            {
                MethodInfo _method = _type.GetMethod(name, BindingFlags.Public | BindingFlags.Static);
                return _method.Invoke(null, parameters);
            }
            else
            {
                throw new Exception("Attempted to access CrewRandR without that mod installed.");
            }
        }
    }
}