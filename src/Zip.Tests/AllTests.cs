// Copyright (C) 2001 Gerry Shaw
//
// This software is provided 'as-is', without any express or implied
// warranty.  In no event will the authors be held liable for any damages
// arising from the use of this software.
//
// Permission is granted to anyone to use this software for any purpose,
// including commercial applications, and to alter it and redistribute it
// freely, subject to the following restrictions:
//
// 1. The origin of this software must not be misrepresented; you must not
//    claim that you wrote the original software. If you use this software
//    in a product, an acknowledgment in the product documentation would be
//    appreciated but is not required.
// 2. Altered source versions must be plainly marked as such, and must not be
//    misrepresented as being the original software.
// 3. This notice may not be removed or altered from any source distribution.
//
// Gerry Shaw (gerry_shaw@yahoo.com)

using System;
using System.Reflection;
using NUnit.Framework;

namespace OrganicBit.Zip.Tests {

	// This class bundles all our tests into a single suite.  If you wanted
	// you could create other suites with a sub set of the tests.  All that is
	// required is a property called Suite that returns a ITest object.  The
	// ITest object most commonly returned is a TestSuite.  For single class
	// tests this member can be included within the TestCase.
	public class AllTests {
		public static ITest Suite {
			get  {
                // Use reflection to automagically scan all the classes that 
                // inherit from TestCase and add them to the suite.
				TestSuite suite = new TestSuite("Zip Tests");
                /*
                Assembly assembly = Assembly.GetExecutingAssembly();
                foreach(Type type in assembly.GetTypes()) {
                    if (type.IsSubclassOf(typeof(TestCase)) && !type.IsAbstract) {
                        suite.AddTestSuite(type);
                    }
                }
                */
                suite.AddTestSuite(typeof(ZipExceptionTest));
                suite.AddTestSuite(typeof(ZipFileTest));
                suite.AddTestSuite(typeof(ZLibTest));
				return suite;
			}
		}
	}
}
