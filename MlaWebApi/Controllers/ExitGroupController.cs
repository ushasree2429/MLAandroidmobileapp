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
using System.Transactions;
using System.Configuration;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace MlaWebApi.Controllers
{
    public class ExitGroupController : ApiController
    {
        public HttpResponseMessage RemoveUser(long userId, long groupId)
        {
            try
            {
                using (TransactionScope scope = new TransactionScope())
                {

                    using (var ctx = new ProjectEntities())
                    {
                        var deleteUser = (from friend in ctx.Friends where friend.userId == userId && friend.GroupId == groupId select friend).FirstOrDefault();

                        ctx.Friends.Remove(deleteUser);
                        ctx.SaveChanges();

                        var deleteFriendUser = (from friend in ctx.Friends where friend.FriendUserId == userId && friend.GroupId == groupId select friend).FirstOrDefault();

                        ctx.Friends.Remove(deleteFriendUser);
                        ctx.SaveChanges();

                        var groupKey = (from grp in ctx.Group_key where grp.user_id == userId && grp.group_id == groupId select grp).FirstOrDefault();
                        ctx.Group_key.Remove(groupKey);
                        ctx.SaveChanges();

                        (from grpversion in ctx.Group_key where grpversion.group_id == groupId select grpversion).ToList().ForEach(x => x.version = x.version + 1);
                        ctx.SaveChanges();
                        scope.Complete();
                    }




                    }
                    var response = Request.CreateResponse<String>(System.Net.HttpStatusCode.OK, "Success");
                return response;
            }
            catch (Exception e)
            {
                var response = Request.CreateResponse<Exception>(System.Net.HttpStatusCode.BadRequest, e);
                return response;
            }

            }

        
        public HttpResponseMessage GetListForDelete(long userId)
        {
            try
            {
                using (var db = new ProjectEntities())
                {

                    string defaultGroup = "defaultgroup";
                    var nestedQuery = (from frnd in db.Friends where (frnd.userId == userId) select frnd.GroupId).ToList();
                    var query = db.Group_table.Where(x => nestedQuery.Contains(x.group_id) && !x.group_name.Contains(defaultGroup) && x.user_id != userId)
                    .Select(p => new
                    {
                        group_name = p.group_name,
                        group_id = p.group_id
                    }).ToList();

                    var response = Request.CreateResponse<IQueryable>(System.Net.HttpStatusCode.OK, query.AsQueryable());
                    return response;


                }

            }
            catch (Exception e)
            {
                var response = Request.CreateResponse<Exception>(System.Net.HttpStatusCode.BadRequest, e);
                return response;
            }


        }
    }
}
