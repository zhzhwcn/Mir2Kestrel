namespace ServerKestrel.Mir2Amz.Objects
{
    internal class Map
    {
        public int Width { get; set; }
        public int Height { get; set; }

        private readonly Models.Map _mapInfo;

        public Models.Map Info => _mapInfo;
        public Cell[,] Cells { get; private set; }

        public Map(Models.Map mapInfo)
        {
            _mapInfo = mapInfo;
            Cells = LoadMapCells(File.ReadAllBytes(_mapInfo.FileName))!;
        }

        private Cell?[,] LoadMapCells(byte[] fileBytes)
        {
            var offSet = 0;
            Width = BitConverter.ToInt16(fileBytes, offSet);
            offSet += 2;
            Height = BitConverter.ToInt16(fileBytes, offSet);
            var cells = new Cell?[Width, Height];
            //DoorIndex = new Door[Width, Height];

            offSet = 52;

            for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
            {//total 12
                if ((BitConverter.ToInt16(fileBytes, offSet) & 0x8000) != 0)
                    cells[x, y] = Cell.HighWall; //Can Fire Over.

                offSet += 2;
                if ((BitConverter.ToInt16(fileBytes, offSet) & 0x8000) != 0)
                    cells[x, y] = Cell.LowWall; //Can't Fire Over.

                offSet += 2;

                if ((BitConverter.ToInt16(fileBytes, offSet) & 0x8000) != 0)
                    cells[x, y] = Cell.HighWall; //No Floor Tile.

                cells[x, y] ??= new Cell (CellAttribute.Walk);

                offSet += 4;

                //if (fileBytes[offSet] > 0)
                //    DoorIndex[x, y] = AddDoor(fileBytes[offSet], new Point(x, y));

                offSet += 3;

                var light = fileBytes[offSet++];

                //if (light >= 100 && light <= 119)
                    //cells[x, y]!.FishingAttribute = (sbyte)(light - 100);
            }

            return cells;
        }

    }

    internal class Cell
    {
        public Cell(CellAttribute attribute)
        {
            if (attribute == CellAttribute.Walk)
            {
                Objects = new List<MapObject>();
            }
        }

        public static Cell LowWall { get; } = new Cell(CellAttribute.LowWall);
        public static Cell HighWall { get; } = new Cell(CellAttribute.HighWall);

        public bool Valid => Attribute == CellAttribute.Walk;

        public List<MapObject>? Objects { get; set; }
        public CellAttribute Attribute { get; init; }

        public void Add(MapObject mapObject)
        {
            Objects?.Add(mapObject);
        }
        public void Remove(MapObject mapObject)
        {
            Objects?.Remove(mapObject);
        }
    }
}
