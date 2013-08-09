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
using OrganicBit.Zip;
using NUnit.Framework;

namespace OrganicBit.Zip.Tests {

    public class ZLibTest : TestCase {

        public ZLibTest(String name) : base(name) {
        }
        
        protected override void SetUp() {
        }
        
        protected override void TearDown() {
        }

        public void Test_Version() {
            try {
                //Assert(ZLib.Version == "1.1.3");
            } catch (Exception) {
            }
        }
    }
}