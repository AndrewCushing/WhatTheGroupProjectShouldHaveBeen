﻿namespace LunaCinemasBackEndInDotNet.Models
{
    public class Film
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public bool IsReleased { get; set; }
        public string Length { get; set; }
        public string BriefDescription { get; set; }
        public string DetailedDescription { get; set; }
        public string ImagePath { get; set; }
        public string Year { get; set; }
        public string Classification { get; set; }
        public string[] Directors { get; set; }
        public string[] Genres { get; set; }
        public string[] Actors { get; set; }
    }
}