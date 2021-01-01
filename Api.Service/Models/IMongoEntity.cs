using System;

namespace Api.Service.Models
{
    public interface IMongoEntity
    {
        public string Id { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}
