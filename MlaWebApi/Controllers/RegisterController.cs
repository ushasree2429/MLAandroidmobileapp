using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MlaWebApi.Models;
using MlaWebApi.EFModels;
////using System.Data.SqlServerCe;
using System.Data;
using System.Configuration;
using System.Windows.Forms;

using System.Data.SqlClient;

namespace MlaWebApi.Controllers
{
    public class RegisterController : ApiController
    { 
        public string cfmgr = ConfigurationManager.ConnectionStrings["database-1"].ConnectionString;
        SqlConnection cnn = null;

        public IEnumerable<Register> GetAllRegister()
        {
            cnn = new SqlConnection(cfmgr);
            cnn.Open();

            SqlCommand comm = new SqlCommand("Select userId, userName, userType  from register", cnn);
            SqlDataAdapter Sqlda = new SqlDataAdapter(comm);
            DataSet dsDatast = new DataSet("register");
            Sqlda.Fill(dsDatast);

            foreach (DataRow row in dsDatast.Tables[0].Rows)
            {
                yield return new Register
                {
                    userId = Int16.Parse(Convert.ToString(row["userId"])),
                    userName = Convert.ToString(row["userName"]),
                    userType = Convert.ToString(row["userType"])
                };
            }

        }

        public IEnumerable<Register> GetRegisterByUserName(string userName)
        {
           
            cnn = new SqlConnection(cfmgr);
            cnn.Open();

            SqlCommand comm = new SqlCommand("Select userId,userName,userType from register where userName = '" + userName + "'", cnn);
            SqlDataAdapter Sqlda = new SqlDataAdapter(comm);

            DataSet dataSet = new DataSet("register");
            Sqlda.Fill(dataSet);

            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                yield return new Register
                 {
                    userId = Int16.Parse(Convert.ToString(row["userId"])),
                    userName = Convert.ToString(row["userName"]),
                    userType = Convert.ToString(row["userType"])
                };
            }
        }


        public IQueryable GetRegisterAuth(string userName, string password)
        {
            var key = "b14ca5898a4e4133bbce2ea2315a1916";
            var str = password;
            var encryptedString = AesOperationController.EncryptString(key, str);
          
            using (var ctx = new ProjectEntities())

            {
                var query = ctx.registers.Where(x => x.userName == userName && x.password == encryptedString)
                 .Select(reg => new
                 {
                     userId = reg.userId,
                     userType = reg.userType,
                     userName = reg.userName
                 }).ToList();
                return query.AsQueryable();
            }
        }

        public IEnumerable<Register> GetRegisterByUserId(int userId)
        {

            cnn = new SqlConnection(cfmgr);
            cnn.Open();

            SqlCommand comm = new SqlCommand("Select userId,userName,userType from register where userId = " + userId, cnn);
            SqlDataAdapter Sqlda = new SqlDataAdapter(comm);

            DataSet dataSet = new DataSet("register");
            Sqlda.Fill(dataSet);

            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                yield return new Register
                {
                    userId = Int16.Parse(Convert.ToString(row["userId"])),
                    userName = Convert.ToString(row["userName"]),
                    userType = Convert.ToString(row["userType"])
                };
            }
        }

        public HttpResponseMessage PostRegisterPassUpdate(string userName, string password)
        {
            var key = "b14ca5898a4e4133bbce2ea2315a1916";
            var str = password;
            var encryptedString = AesOperationController.EncryptString(key, str);

            DataSet dsData = new DataSet("register");
            cnn = new SqlConnection(cfmgr);
            cnn.Open();

            Register register = new Register();
            register.userName = userName;
            try
            {
                SqlCommand comm = new SqlCommand("Update register set password ='" + encryptedString + "'"+" where userName = '"+userName+"'", cnn);
                //int countUpdated =comm.ExecuteNonQuery();
                SqlDataAdapter sqlada = new SqlDataAdapter(comm);
                sqlada.Fill(dsData);
            //    comm.ExecuteNonQuery();
              //  comm.Dispose();

                var response = Request.CreateResponse<Register>(System.Net.HttpStatusCode.Found, register);
                cnn.Close();
                return response;
            }
            catch (Exception e)
            {
                var response = Request.CreateResponse<Register>(System.Net.HttpStatusCode.BadRequest, register);
                cnn.Close();
                return response;
            }
            
        }

        public HttpResponseMessage PostRegister(Register register)
        {
            //password encrypt
              var key = "b14ca5898a4e4133bbce2ea2315a1916";
              var str = register.password;
              var encryptedString = AesOperationController.EncryptString(key, str);

            DataSet dsData = new DataSet("register");
            cnn = new SqlConnection(cfmgr);
            cnn.Open();

            try
            {
                SqlCommand comm = new SqlCommand("Insert into register(userName,password,userType) values('"
                    + register.userName
                    + "','" + encryptedString
                    + "','" + register.userType
                    + "')", cnn);
                SqlDataAdapter sqlada = new SqlDataAdapter(comm);
                sqlada.Fill(dsData);

                
                var response = Request.CreateResponse<Register>(System.Net.HttpStatusCode.Created, register);

                return response;
            }
            catch (Exception e)
            {
                var response = Request.CreateResponse<Register>(System.Net.HttpStatusCode.BadRequest, register);
                return response;
            }

        }

        public HttpResponseMessage PostAddInstructor(string instUserName, string instPassword, string instFirsName, string instLastName, string instTelephone, string instAddress, string instAliasMailId, string instEmailId, string instSkypeId,string publicKey,string groupkey)
        {
            var key = "b14ca5898a4e4133bbce2ea2315a1916";
            var str = instPassword;
            var encryptedString = AesOperationController.EncryptString(key, str);

            DataSet dsData = new DataSet("register");
            cnn = new SqlConnection(cfmgr);
            cnn.Open();
            string userType = "instructor"; // userType = instructor or student or admin
            int userId = 0;

            //first add to register table then to the instructor table.
            try
            {
                SqlCommand comm = new SqlCommand("Insert into register(userName,password,userType,publickey) values('"
                    + instUserName
                    + "','" + encryptedString
                    + "','" + userType
                     + "','" + publicKey
                    + "')", cnn);
                SqlDataAdapter sqlada = new SqlDataAdapter(comm);
                sqlada.Fill(dsData);

                // retrive the userId since it is auto incremented in the database and need to be added to the instructor table
                comm = new SqlCommand("select userId from register where userName = '" + instUserName + "'",cnn);
                sqlada = new SqlDataAdapter(comm);
                sqlada.Fill(dsData);


                foreach (DataRow row in dsData.Tables[0].Rows)
                {
                    userId = Int16.Parse(Convert.ToString(row["userId"]));
                }

            }
            catch (Exception e)
            {
                Instructor emptyInst = new Instructor();
                var response = Request.CreateResponse<Instructor>(System.Net.HttpStatusCode.BadRequest, emptyInst);
                return response;
            }
            using (var ctx = new ProjectEntities())
            {
                var user = ctx.registers.Where(x => x.userName == instUserName).Select(reg => new
                {
                    userId = reg.userId
                }).ToList();
                userId = Convert.ToInt32(user[0].userId);
                var groupName = "defaultgroup" + userId;
                Group_table grpObj = new Group_table();
                grpObj.group_name = groupName;
                grpObj.user_id = userId;
                grpObj.status = "Active";

                ctx.Group_table.Add(grpObj);
                ctx.SaveChanges();
                creategroupkey(groupName, userId, groupkey);
            }


            Instructor inst = new Instructor();
            inst.idInstructor = instUserName;
            inst.firstName = instFirsName;
            inst.lastName = instLastName;
            inst.userId = userId;
            inst.telephone = instTelephone;
            inst.address = instAddress;
            inst.aliasMailId = instAliasMailId;
            inst.emailId = instEmailId;
            inst.skypeId = instSkypeId;
            // now add to instructor table.
            try
            {
                SqlCommand comm = new SqlCommand("Insert into instructor(idInstructor,firstName,lastName,userId,telephone,address,aliasMailId,emailId, skypeId) values('"
                    + inst.idInstructor
                    + "','" + inst.firstName
                    + "','" + inst.lastName
                    + "','" + inst.userId
                    + "','" + inst.telephone
                    + "','" + inst.address
                    + "','" + inst.aliasMailId
                    + "','" + inst.emailId
                    + "','" + inst.skypeId
                    + "')", cnn);
                SqlDataAdapter sqlada = new SqlDataAdapter(comm);
                sqlada.Fill(dsData);
                cnn.Close();
                var response = Request.CreateResponse<Instructor>(System.Net.HttpStatusCode.Created, inst);
                return response;
            }
            catch (Exception e)
            {
                var response = Request.CreateResponse<Instructor>(System.Net.HttpStatusCode.BadRequest, inst);
                cnn.Close();
                return response;
            }
        }

        public IQueryable creategroupkey(String groupName, long userId, String groupKey)
        {
            long groupid = 0;
            using (var ctx = new ProjectEntities())
            {
                var user = ctx.Group_table.Where(x => x.group_name == groupName).Select(grp => new
                {
                    groupid = grp.group_id
                }).ToList();
                groupid = Convert.ToInt32(user[0].groupid);


                Group_key gk = new Group_key();
                gk.group_id = groupid;
                gk.group_key1 = groupKey;
                gk.status = true;
                gk.user_id = userId;
                gk.version = 1;
                ctx.Group_key.Add(gk);
                ctx.SaveChanges();

                return user.AsQueryable();
            }
        }
        public HttpResponseMessage PostAddStudent(string userName, string password, string firsName, string lastName, string telephone, string address, string aliasMailId, string emailId, string skypeId,string publickey,string groupKey)
        {

            var key = "b14ca5898a4e4133bbce2ea2315a1916";
            var str = password;
            var encryptedString = AesOperationController.EncryptString(key, str);
            DataSet dsData = new DataSet("register");
            cnn = new SqlConnection(cfmgr);
            cnn.Open();
            string userType = "student"; // userType = instructor or student or admin
            int userId = 0;

            //first add to register table then to the student table.
            try
            {
                SqlCommand comm = new SqlCommand("Insert into register(userName,password,userType,publicKey) values('"
                    + userName
                    + "','" + encryptedString
                    + "','" + userType
                     + "','" + publickey
                    + "')", cnn);
                SqlDataAdapter sqlada = new SqlDataAdapter(comm);
                sqlada.Fill(dsData);

                // retrive the userId since it is auto incremented in the database and need to be added to the student table
                comm = new SqlCommand("select userId from register where userName = '" + userName + "'",cnn);
                sqlada = new SqlDataAdapter(comm);
                sqlada.Fill(dsData);


                foreach (DataRow row in dsData.Tables[0].Rows)
                {
                    userId = Int16.Parse(Convert.ToString(row["userId"]));
                }

            }
            catch (Exception e)
            {
                Student emptyStud = new Student();
                var response = Request.CreateResponse<Student>(System.Net.HttpStatusCode.BadRequest, emptyStud);
                cnn.Close();
                return response;
            }

            using (var ctx = new ProjectEntities())
            {
                var user = ctx.registers.Where(x => x.userName == userName).Select(reg => new
                {
                    userId = reg.userId
                }).ToList();
                userId = Convert.ToInt32(user[0].userId);
                var groupName = "defaultgroup" + userId;
                Group_table grpObj = new Group_table();
                grpObj.group_name = groupName;
                grpObj.user_id = userId;
                grpObj.status = "Active";

                ctx.Group_table.Add(grpObj);
                ctx.SaveChanges();
                creategroupkey(groupName, userId, groupKey);
            }

            Student stud = new Student();
            stud.idStudent = userName;
            stud.firstName = firsName;
            stud.lastName = lastName;
            stud.userId = userId;
            stud.telephone = telephone;
            stud.address = address;
            stud.aliasMailId = aliasMailId;
            stud.emailId = emailId;
            stud.skypeId = skypeId;
            // now add to instructor table.
            try
            {
                SqlCommand comm = new SqlCommand("Insert into student(idStudent,firstName,lastName,userId,telephone,address,aliasMailId,emailId,skypeId) values('"
                    + stud.idStudent
                    + "','" + stud.firstName
                    + "','" + stud.lastName
                    + "','" + stud.userId
                    + "','" + stud.telephone
                    + "','" + stud.address
                    + "','" + stud.aliasMailId
                    + "','" + stud.emailId
                    + "','" + stud.skypeId
                    + "')", cnn);
                SqlDataAdapter sqlada = new SqlDataAdapter(comm);
                sqlada.Fill(dsData);
                cnn.Close();
                var response = Request.CreateResponse<Student>(System.Net.HttpStatusCode.Created, stud);
                return response;
            }
            catch (Exception e)
            {
                var response = Request.CreateResponse<Student>(System.Net.HttpStatusCode.BadRequest, stud);
                cnn.Close();
                return response;
            }
        }
        
        public HttpResponseMessage PostAddAdmin(string adminUserName, string adminPassword, string adminFirsName, string adminLastName, string adminTelephone, string adminAddress, string adminAliasMailId, string adminEmailId, string adminSkypeId, string publicKey,string groupKey)
        {
            var key = "b14ca5898a4e4133bbce2ea2315a1916";
            var str = adminPassword;
            var encryptedString = AesOperationController.EncryptString(key, str);

            DataSet dsData = new DataSet("admin");
            cnn = new SqlConnection(cfmgr);
            cnn.Open();
            string userType = "admin"; // userType = instructor or student or admin
            int userId = 0;
            //  MessageBox.Show(adminUserName);
            //Console.WriteLine(adminUserName);
            //first add to register table then to the student table.
            using (var ctx = new ProjectEntities())
            {
                register objreg = new register();
                objreg.userName = adminUserName;
                objreg.password = encryptedString;
                objreg.userType = userType;
                objreg.publickey = publicKey;
                ctx.registers.Add(objreg);
                ctx.SaveChanges();

              
                var user = ctx.registers.Where(x => x.userName == adminUserName).Select(reg => new
                {
                    userId = reg.userId
                }).ToList();
                userId = Convert.ToInt32(user[0].userId);

                var groupName = "defaultgroup" + userId;
                Group_table grpObj = new Group_table();
                grpObj.group_name = groupName;
                grpObj.user_id = userId;
                grpObj.status = "Active";

                ctx.Group_table.Add(grpObj); 
                ctx.SaveChanges();
                creategroupkey(groupName, userId, groupKey);
            }
            
            Admin adm = new Admin();
            adm.idAdmin = adminUserName;
            adm.firstName = adminFirsName;
            adm.lastName = adminLastName;
            adm.userId = userId;
            adm.telephone = adminTelephone;
            adm.address = adminAddress;
            adm.aliasMailId = adminAliasMailId;
            adm.emailId = adminEmailId;
            adm.skypeId = adminSkypeId;
            // now add to instructor table.
            

            try
            {
                SqlCommand comm = new SqlCommand("Insert into admin(idAdmin,firstName,lastName,userId,telephone,address,aliasMailId,emailId,skypeId) values('"
                    + adm.idAdmin
                    + "','" + adm.firstName
                    + "','" + adm.lastName
                    + "','" + adm.userId
                    + "','" + adm.telephone
                    + "','" + adm.address
                    + "','" + adm.aliasMailId
                    + "','" + adm.emailId
                    + "','" + adm.skypeId
                    + "')", cnn);
                SqlDataAdapter sqlada = new SqlDataAdapter(comm);
                sqlada.Fill(dsData);
                cnn.Close();
                var response = Request.CreateResponse<Admin>(System.Net.HttpStatusCode.Accepted, adm);
                return response;
            }
            catch (Exception e)
            {
                var response = Request.CreateResponse<Admin>(System.Net.HttpStatusCode.BadRequest, adm);
                cnn.Close();
                return response;
            }
        }
        
    }
}
