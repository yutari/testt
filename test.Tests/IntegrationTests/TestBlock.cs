using Xunit;
using Autodesk.AutoCAD.DatabaseServices;
using test.Data;
using test.Services;
using test.Tests.Helpers;
using System.Collections.Generic;

namespace test.Tests.IntegrationTests
{
    public class TestBlock : GeometryTestBase
    {
        [Fact]
        public void InsertBlock_FromExternalFile()
        {
            ClearModelSpace();

            var batch = new BlockBatch
            {
                Blocks = new List<BlockInsertItem>
                {
                    new BlockInsertItem
                    {
                        FilePath = @"D:/Tài liệu/Đi làm/thietkecua/ThietKeCuaNhua.dwg",
                        BlockName = "HinhChieuBangABS_140_2NepTC",
                        Position = new PointSchema { X = 0, Y = 0, Z = 0 },
                        Scale = 1.0,
                        Rotation = 0.0
                    }
                }
            };

            var ids = BlockService.InsertBlocks(_db, batch);

            Assert.Single(ids);
            Assert.True(ExistsInModelSpace<BlockReference>(br =>
                br.Name == "HinhChieuBangABS_140_2NepTC" &&
                br.Position.X == 0 && br.Position.Y == 0));
        }

        [Fact]
        public void InsertMultipleBlocks_FromExternalFile()
        {
            ClearModelSpace();

            var batch = new BlockBatch
            {
                Blocks = new List<BlockInsertItem>
                {
                    new BlockInsertItem
                    {
                        FilePath = @"D:/Tài liệu/Đi làm/thietkecua/ThietKeCuaNhua.dwg",
                        BlockName = "HinhChieuBangABS_140_2NepTC",
                        Position = new PointSchema { X = 0, Y = 0, Z = 0 },
                        Scale = 1.0,
                        Rotation = 0.0
                    },
                    new BlockInsertItem
                    {
                        FilePath = @"D:/Tài liệu/Đi làm/thietkecua/ThietKeCuaNhua.dwg",
                        BlockName = "HinhChieuBangABS_140_2NepTC",
                        Position = new PointSchema { X = 200, Y = 100, Z = 0 },
                        Scale = 0.5,
                        Rotation = 0.25
                    }
                }
            };

            var ids = BlockService.InsertBlocks(_db, batch);

            Assert.Equal(2, ids.Count);
            Assert.True(ExistsInModelSpace<BlockReference>(br =>
                br.Position.X == 200 &&
                br.Position.Y == 100));
        }
    }
}

