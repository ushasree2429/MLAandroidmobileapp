using MlaWebApi.EFModels;
using MlaWebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Transactions;

namespace MlaWebApi.Controllers
{
    public class GroupController : ApiController
    {
        public void CreateDefaultGroup(string groupKey, long userId)
        {
            try
            {
                using (TransactionScope scope = new TransactionScope())
            {
                var groupName = "group" + userId;
                using (var ctx = new ProjectEntities())

                {
                    Group_table grpObj = new Group_table();
                    grpObj.group_name = groupName;
                    grpObj.user_id = userId;
                    grpObj.status = "Active";

                    ctx.Group_table.Add(grpObj);
                    ctx.SaveChanges();
                    scope.Complete();
                    creategroupkey(groupName, userId, groupKey);



                }
            }
            }
            catch (Exception e)
            {
                var response = Request.CreateResponse<Exception>(System.Net.HttpStatusCode.BadRequest, e);
                
            }


        }

        public HttpResponseMessage creategroupkey(String groupName, long userId, String groupKey)
        {

            try
            {
                using (TransactionScope scope = new TransactionScope())
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

                        scope.Complete();                                                                                                              
                       // return user.AsQueryable();
                        return Request.CreateResponse<IQueryable>(System.Net.HttpStatusCode.OK, user.AsQueryable());
                    }
                }
            }

            catch (Exception e)
            {
                var response = Request.CreateResponse<Exception>(System.Net.HttpStatusCode.BadRequest, e);
                return response;

            }
        }

        public HttpResponseMessage GetFriendsList(long userId)
        {
            try
            {
                using (var ctx = new ProjectEntities())
                {

                    var innerQuery = (from frnd in ctx.Friends where frnd.userId == userId select frnd.FriendUserId);
                    var res = ctx.registers.Where(x => innerQuery.Contains(x.userId)).Select(reg => new
                    {
                        userId = reg.userId,
                        userType = reg.userType,
                        userName = reg.userName
                    }).ToList();

                  //  return res.AsQueryable();
                    return Request.CreateResponse<IQueryable>(System.Net.HttpStatusCode.OK, res.AsQueryable());
                }
            }
            catch (Exception e)
            {
                return Request.CreateResponse<Exception>(System.Net.HttpStatusCode.BadRequest, e);
            }
        }

        

        public HttpResponseMessage CreateGroup([FromBody] Group objGrp)
        {
            long grpId = 0;
            try
            {

                using (TransactionScope scope = new TransactionScope())
                {
                    using (var ctx = new ProjectEntities())
                    {
                        Group_table grpObj = new Group_table();
                        grpObj.group_name = objGrp.groupName;
                        grpObj.user_id = objGrp.userId;
                        grpObj.status = "Active";

                        ctx.Group_table.Add(grpObj);
                        ctx.SaveChanges();

                        var user = ctx.Group_table.Where(x => x.group_name == objGrp.groupName).Select(grp => new
                        {
                            grpId = grp.group_id
                        }).ToList();

                        grpId = Convert.ToInt32(user[0].grpId);

                        Group_key gk = new Group_key();

                        gk.group_id = grpId;
                        gk.group_key1 = objGrp.groupKey;
                        gk.status = true;
                        gk.user_id = objGrp.userId;
                        gk.version = 1;
                        ctx.Group_key.Add(gk);
                        ctx.SaveChanges();
                    }
                    using (var db = new ProjectEntities())
                    {
                        for (int i = 0; i < objGrp.friend_ids.Count; i++)
                        {
                            Friend frnd = new Friend();
                            frnd.userId = objGrp.userId;
                            frnd.GroupId = grpId;
                            frnd.FriendUserId = objGrp.friend_ids[i];
                            db.Friends.Add(frnd);
                            db.SaveChanges();

                            var fr_id = Convert.ToInt32(objGrp.friend_ids[i]);

                            Friend userFrnd = new Friend();
                            userFrnd.FriendUserId = objGrp.userId;
                            userFrnd.GroupId = grpId;
                            userFrnd.userId = fr_id;
                            db.Friends.Add(userFrnd);
                            db.SaveChanges();


                            Group_key groupKey = new Group_key();
                            groupKey.group_id = grpId;
                            groupKey.group_key1 = objGrp.group_keys[i];
                            groupKey.user_id = fr_id;
                            groupKey.status = true;
                            groupKey.version = 1;
                            db.Group_key.Add(groupKey);
                            db.SaveChanges();
                          

                        }
                    }
                    scope.Complete();
                    return Request.CreateResponse<String>(System.Net.HttpStatusCode.Accepted, "Created");
                }
            }
            catch (Exception e)
            {
                return Request.CreateResponse<Exception>(System.Net.HttpStatusCode.BadRequest, e);
            }

        }

    }
}




