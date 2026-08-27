namespace StudentMangmentSystem_API
{
    public class ResponseApi<TData>
    {
        public bool IsSuccessed { get; set; }
        public int StatusCode { get; set; }
        public string Massage { get; set; }
        public object? Error { get; set; }
        public TData? Value { get; set; }


        public static ResponseApi<TData> Create(bool IsSuccessed, int statuscode, string Massage,
            TData? Value = default, object? Error = null)
        {
            return new ResponseApi<TData>
            {
                StatusCode = statuscode,
                Massage = Massage,
                IsSuccessed = IsSuccessed,
                Value = Value,
                Error = Error
            };
        }

        public static ResponseApi<TData> BadRequst(string massage, object? error=null)
            => Create(false, 400, massage, Error : error);

        public static ResponseApi<TData> NotFound(string massage="The Resource Not Found")
            => Create(false, 404, massage);

        public static ResponseApi<TData> Conflict(string massage)
            => Create(false, 409, massage);

        public static ResponseApi<TData> Ok(TData? data,string massage)
          => Create(true, 200, massage,data);
        public static ResponseApi<TData> CreatedAt(TData? data, string massage)
          => Create(true, 201, massage, data);
        public static ResponseApi<TData> NoContant(TData? data, string massage="The Task Went Perfect")
          => Create(true, 204, massage, data);



    }
}
