import express from "express";
import { Book } from "../db/sequelize.mjs";
import { sucess } from "./helper.mjs";
import { ValidationError, Op } from "sequelize";
import { auth } from "../auth/auth.mjs";

const booksRouter = express();

booksRouter.get("/", auth, (req, res) => {
  if (req.query.title) {
    if (req.query.title.length < 4) {
      const message = `Le terme de la recherche doit contenir au moins 4 caractères`;
      return res.status(400).json({ message });
    }
    let limit = 6;
    if (req.query.limit) {
      limit = parseInt(req.query.limit, 10);
    }
    return Book.findAll({
      where: { title: { [Op.like]: `%${req.query.title}%` } },
      attributes: { exclude: ["content"] },
      order: ["title"],
      limit: limit,
    }).then((Books) => {
      const message = `Il y a ${Books.length} produits qui correspondent au terme de la recherche`;
      res.json(sucess(message, Books));
    });
  }
  Book.findAll({
    attributes: { exclude: ["content"] },
    order: ["title"],
  })
    .then((Books) => {
      const message = "La liste des produits a bien été récupérée. ";
      res.json(sucess(message, Books));
    })
    .catch((error) => {
      const message =
        "La liste des produits n'a pas été récupérée. Merci de réessayer dans quelques instants.";
      res.status(500).json({ message, data: error });
    });
});

booksRouter.get("/:id", auth, (req, res) => {
  Book.findByPk(req.params.id)
    .then((Books) => {
      if (Books === null) {
        const message =
          "Le produit demandé n'existe pas. Merci de réessayer avec une autre identifiant.";
        return res.status(404).json({ message });
      }
      const message = `Le produit dont l'id vaut ${Books.id} a bien été récupérée`;
      res.json(sucess(message, Books));
    })
    .catch((error) => {
      const message =
        "Le produit n'a pas pu être récupéré. Merci de réessayer dans quelques instants.";
      res.status(500).json({ message, data: error });
    });
});

booksRouter.post("/", auth, (req, res) => {
  Book.create(req.body)
    .then((createdBook) => {
      const message = `Le produit ${createdBook.title} a bien été crée !`;
      res.json(sucess(message, createdBook));
    })
    .catch((error) => {
      if (error instanceof ValidationError) {
        return res.status(400).json({ message: error.message, data: error });
      }
      const message =
        "Le produit n'a pas pu être ajouté. Merci de réessayer dans quelques instants.";
      res.status(500).json({ message, data: error });
    });
});

booksRouter.put("/:id", auth, (req, res) => {
  const BookId = req.params.id;
  Book.update(req.body, { where: { id: BookId } })
    .then((_) => {
      return Book.findByPk(BookId).then((updateBook) => {
        if (updateBook === null) {
          const message =
            "Le produit demandé n'existe pas. Merci de réessayer avec un autre identifiant.";
          return res.status(404).json({ message });
        }
        const message = `Le produit ${updateBook.title} a bien été modifié`;
        res.json(sucess(message, updateBook));
      });
    })
    .catch((error) => {
      const message =
        "Le produit n'a pas pu être mis à jour. Merci de réessayer dans quelques instants.";
      res.status(500).json({ message, data: error });
    });
});

booksRouter.delete("/:id", auth, (req, res) => {
  Book.findByPk(req.params.id)
    .then((deleteBook) => {
      if (deleteBook == null) {
        const message =
          "Le produit demandé n'existe pas. Merci de réessayer avec un autre identifiant";
        return res.status(404).json({ message });
      }
      return Book.destroy({
        where: { id: deleteBook.id },
      }).then((_) => {
        const message = `Le produit ${deleteBook.title} a bien été supprimé`;
        res.json(sucess(message, deleteBook));
      });
    })
    .catch((error) => {
      const message =
        "Le produit n'a pas pu être supprimé. Merci de réessayer dans quelques instants.";
      res.status(500).json({ message, data: error });
    });
});

export { booksRouter };
