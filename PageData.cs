namespace nkport_api
{
    //Ugliest data structure ever.
    //You have to know the structure of the database in order to utilize any of this sadly. 
    //This will change in the future after the designer revises the final 6 pages (12 total pages). 
    public class CarouselCard{
        public int CarouselID { get; set; }
        public string? Image { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
    }

    public class ContainerData{
        public int Container { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public string? Title2 { get; set; }
        public string? Description2 { get; set; }
        public string? ImageBLeft { get; set; }
        public string? ImageMidRight { get; set; }
        public string? Title3 { get; set; }
        public string? Description3 { get; set; }
        public string? Video { get; set; }
        public string? Resume { get; set; }
        public string? Email { get; set; }
        public string? Copyright { get; set; }
        public List<CarouselCard>? CarouselCards { get; set; }
    }

    public class PageData{
        public int PageID { get; set; }
        public List<ContainerData>? Containers { get; set; }
        public string? BlobAppendSAS { get; set; }
        public string? BlobContainer { get; set; }
    }
}