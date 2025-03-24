using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace SQLExtends.EFCore;

public class Paginate<T> : List<T>
{
    [JsonPropertyName("pageNum")]
    [JsonProperty("pageNum")]
    public int PageNum { get; set; }
    
    [JsonPropertyName("pageSize")]
    [JsonProperty("pageSize")]
    public int PageSize { get; set; }
    
    [JsonPropertyName("totalPages")]
    [JsonProperty("totalPages")]
    public int TotalPages { get; set; }

    public Paginate() { 

    }

    public Paginate(IList<T> items,int count, int pageNum, int pageSize)
    {
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        PageNum = Math.Min(TotalPages, Math.Max(pageNum, 1));

        AddRange(items);
    }

    public int PreviousPage => Math.Max(PageNum - 1, 1); 
    public bool HasPreviousPage => PageNum > 1;

    public int NextPage => Math.Min(PageNum + 1, TotalPages);
    public bool HasNextPage => PageNum < TotalPages;

    public static Paginate<T> CreateCustom(IList<T> source, int pageNum, int pageSize, int count)
    {
        var paginate = new Paginate<T>();        
        
        paginate.PageSize = pageSize;
        paginate.TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        paginate.PageNum = Math.Min(paginate.TotalPages, Math.Max(pageNum,1));
        paginate.AddRange(source);

        return paginate;        
    }
    
    public static Paginate<T> CreateCustom(IList<T> source, int pageNum, int pageSize)
    {
        var paginate = new Paginate<T>();
        var count = source.Count;
        
        paginate.PageSize = pageSize;
        paginate.TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        paginate.PageNum = Math.Min(paginate.TotalPages, Math.Max(pageNum,1));
        
        var skip = source.Skip((paginate.PageNum -1) * pageSize).Take(pageSize);
        paginate.AddRange(skip);

        return paginate;        
    }
}