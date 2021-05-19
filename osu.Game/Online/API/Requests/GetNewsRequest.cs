﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.IO.Network;
using osu.Game.Extensions;

namespace osu.Game.Online.API.Requests
{
    public class GetNewsRequest : APIRequest<GetNewsResponse>
    {
        private readonly int year;
        private readonly Cursor cursor;

        public GetNewsRequest(int year = 0, Cursor cursor = null)
        {
            this.year = year;
            this.cursor = cursor;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();
            req.AddCursor(cursor);

            if (year != 0)
                req.AddParameter("year", year.ToString());

            return req;
        }

        protected override string Target => "news";
    }
}
