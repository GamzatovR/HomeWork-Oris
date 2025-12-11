using Moq;
using MyORMLibrary;
using System.Data;

namespace MyORMLibraryTest
{
    [TestClass]
    public sealed class ORMContextTests
    {
        private ORMContext orm;
        private const string TableName = "Users";

        [TestInitialize]
        public void Setup()
        {
            string connStr = "Host=localhost;Port=5432;Database=BdToOris;Username=postgres;Password=Gamza_2109_KFU";
            orm = new ORMContext(connStr);
        }

        //[TestMethod]
        //public void Create_Just_Create()
        //{
        //    var entity = new User();
        //    var expectedEntity = new User { id = 1 };
        //    var result = orm.Create<User>(entity, TableName);
        //    Assert.AreEqual(expectedEntity, result);
        //}
        //[TestMethod]
        //public void Delete_Just_Delete()
        //{
        //    orm.Delete(2, "Users");
        //    var expected = orm.ReadByAll<User>(TableName);
        //    Assert.AreEqual(expected.Count, 0);
        //}
        //[TestMethod]
        //public void ReadById_WhenEntityFull()
        //{
        //    var testUser = new User
        //    {
        //        UserName = "test_user",
        //        Age = 25,
        //        Password = "qwerty"
        //    };

        //    var created = orm.Create(testUser, TableName);
        //    int id = created.id;

        //    var userFromDb = orm.ReadById<User>(id, TableName);

        //    Assert.IsNotNull(userFromDb);
        //    Assert.AreEqual(id, userFromDb.id);
        //    Assert.AreEqual("test_user", userFromDb.UserName);
        //    Assert.AreEqual(25, userFromDb.Age);
        //    Assert.AreEqual("qwerty", userFromDb.Password);
        //}
        
    }
}
