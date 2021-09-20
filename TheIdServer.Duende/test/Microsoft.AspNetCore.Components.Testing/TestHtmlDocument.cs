// Copyright (c) 2021 @Olivier Lefebvre. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using HtmlAgilityPack;

namespace Microsoft.AspNetCore.Components.Testing
{
    internal class TestHtmlDocument : HtmlDocument
    {
        public TestHtmlDocument(TestRenderer renderer)
        {
            Renderer = renderer;
        }

        public TestRenderer Renderer { get; }
    }
}
